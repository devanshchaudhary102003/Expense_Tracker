using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);//create new local user, return JWT.
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);//verify password, return JWT
        Task<AuthResponseDTO> LoginOrRegisterExternalAsync(string provider, string externalId, string email, string fullName);//handle Google login (find existing or create).
        Task<UserResponseDTO> GetByIdAsync(int userId);//fetch one user.
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();//admin: list all users.
        Task<UserResponseDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto);//change name and/or currency.
        Task ChangePasswordAsync(int userId, ChangePasswordDTO dto);//verify old, set new.
        Task DeactivateAccountAsync(int userId);//soft delete (set IsActive = false).
        Task DeleteAccountAsync(int userId);//hard delete (admin only).

        AuthResponseDTO BuildAuthResponse(UserEntity user);//generate JWT and wrap user info in response DTO
    }
}