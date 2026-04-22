using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Interface;
using AuthService.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly AuthDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthServiceImpl> _logger;

        public AuthServiceImpl(AuthDbContext db, IConfiguration config, ILogger<AuthServiceImpl> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        // ---------------- Register ----------------

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            var emailNorm = dto.Email.Trim().ToLowerInvariant();

            if(await _db.Users.AnyAsync(u => u.Email == emailNorm))
                throw new InvalidOperationException("Email is already registered.");

            var user = new UserEntity
            {
                FullName = dto.FullName.Trim(),
                Email = emailNorm,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "INR" : dto.Currency.ToUpperInvariant(),
                Role = "User",
                AuthProvider = "Local",
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User registered: {Email} (id={UserId})", user.Email, user.UserId);
            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var emailNorm = dto.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailNorm)
                    ?? throw new UnauthorizedAccessException("Invalid Email or Password.");

             if(!user.IsActive)
                throw new UnauthorizedAccessException("Account is suspended.");

            if(!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))  
                throw new UnauthorizedAccessException("Invalid email or password");  

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return BuildAuthResponse(user);   
        }

        public async Task<AuthResponseDTO> LoginOrRegisterExternalAsync(string provider, string externalId, string email, string fullName, string? avatarUrl)
        {
            var emailNorm = email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.AuthProvider == provider && u.ExternalId == externalId);

            if(user == null)
                user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailNorm);

            if(user == null)
            {
                user = new UserEntity
                {
                    FullName = fullName,
                    Email = emailNorm,
                    PasswordHash = provider,
                    AuthProvider = provider,
                    ExternalId = externalId,
                    AvatarUrl = avatarUrl,
                    Currency = "INR",
                    Role = "User",
                    IsActive = true
                };
                _db.Users.Add(user);
            }

            else
            {
                if (string.IsNullOrEmpty(user.ExternalId))
                {
                    user.AuthProvider = provider;
                    user.ExternalId = externalId;
                }

                if(string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(avatarUrl))
                    user.AvatarUrl = avatarUrl;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return BuildAuthResponse(user);
        }

        public async Task<UserResponseDTO> GetByIdAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
                        ?? throw new KeyNotFoundException($"User {userId} not found.");
            return ToResponse(user);
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync() => (await _db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync()).Select(ToResponse);

        public async Task<UserResponseDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await _db.Users.FindAsync(userId)
                        ?? throw new KeyNotFoundException($"User {userId} not found.");

            if(!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName.Trim();
            if(dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            if(!string.IsNullOrWhiteSpace(dto.Currency)) user.Currency = dto.Currency;

            await _db.SaveChangesAsync();
            return ToResponse(user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            var user = await _db.Users.FindAsync(userId)
                        ?? throw new KeyNotFoundException($"User {userId} not found.");

            if(user.AuthProvider != "Local")
                throw new InvalidOperationException("Password cannot be change for OAuth accounts.");

            if(!PasswordHelper.VerifyPassword(dto.OldPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAccountAsync(int userId)
        {
            await _db.Users.Where(u => u.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false));
        }

        public async Task DeleteAccountAsync(int userId)
        {
            await _db.Users.Where(u => u.UserId == userId).ExecuteDeleteAsync();
        }

        public AuthResponseDTO BuildAuthResponse(UserEntity user)
        {
            var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret missing.");
            var issuer = _config["Jwt:Issuer"] ?? "SpendSmart";
            var audience = _config["Jwt:Audience"] ?? "SpendSmart";
            var hours = int.Parse(_config["Jwt:ExpiryHours"] ?? "24");
            var expiresAt = DateTime.UtcNow.AddHours(hours);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("currency",user.Currency)  
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience:audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDTO(
                user.UserId, user.FullName, user.Email, user.Role, user.Currency, jwt, expiresAt
            );
        }

        private static UserResponseDTO ToResponse(UserEntity u) => new(
            u.UserId, u.FullName, u.Email, u.Currency, u.AvatarUrl, u.Role,
            u.IsActive, u.AuthProvider, u.CreatedAt, u.LastLoginAt
        );
    }
}