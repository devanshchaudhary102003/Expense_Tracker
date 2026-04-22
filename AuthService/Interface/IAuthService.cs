using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<AuthResponseDTO> LoginOrRegisterExternalAsync(string provider, string externalId, string email, string fullName, string? avatarUrl);
        Task<UserResponseDTO> GetByIdAsync(int userId);
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDTO dto);
        Task DeactivateAccountAsync(int userId);
        Task DeleteAccountAsync(int userId);

        AuthResponseDTO BuildAuthResponse(UserEntity user);
    }
}