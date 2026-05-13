using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO dto) =>
            Ok(await _auth.RegisterAsync(dto));

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto) =>
            Ok(await _auth.LoginAsync(dto));

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDTO>> Me() =>
            Ok(await _auth.GetByIdAsync(CurrentUserId()));

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserResponseDTO>> UpdateProfile(UpdateProfileDTO dto) =>
            Ok(await _auth.UpdateProfileAsync(CurrentUserId(), dto));

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            await _auth.ChangePasswordAsync(CurrentUserId(), dto);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("deactivate")]
        public async Task<IActionResult> Deactivate()
        {
            await _auth.DeactivateAccountAsync(CurrentUserId());
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers() =>
            Ok(await _auth.GetAllUsersAsync());

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id:int}")]
        public async Task<ActionResult<UserResponseDTO>> GetUser(int id) =>
            Ok(await _auth.GetByIdAsync(id));

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _auth.DeleteAccountAsync(id);
            return NoContent();
        }

        // ---------- Google OAuth ----------
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
        {
            var redirect = Url.Action(nameof(GoogleCallback), new { returnUrl });
            var props = new AuthenticationProperties { RedirectUri = redirect };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
        {
            // After Google OAuth, identity is stored in the Cookie scheme, not Google scheme
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
                return Unauthorized(new { message = "Google authentication failed." });

            var external = ExtractExternalUser(result.Principal, "Google");
            if (external == null)
                return BadRequest(new { message = "Google did not return required claims." });

            var resp = await _auth.LoginOrRegisterExternalAsync(
                "Google", external.ExternalId, external.Email, external.FullName
            );

            // Clean up the external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect($"{returnUrl}?token={resp.Token}");

            return Ok(resp);
        }

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("User identifier missing in token"));

        private static ExternalUser? ExtractExternalUser(ClaimsPrincipal principal, string provider)
        {
            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.FindFirst("urn:github:email")?.Value;
            var fullName = principal.FindFirst(ClaimTypes.Name)?.Value
                        ?? principal.FindFirst("name")?.Value
                        ?? "User";

            if (string.IsNullOrEmpty(externalId) || string.IsNullOrEmpty(email))
                return null;

            return new ExternalUser(externalId, email, fullName);
        }

        private record ExternalUser(string ExternalId, string Email, string FullName);
    }
}
