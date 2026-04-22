using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace AuthService.DTOs
{
    public record RegisterDTO(
        [Required, MaxLength(150)] string FullName,
        [Required, EmailAddress] string Email,
        [Required, MinLength(6)] string Password,
        string? Currency
    );

    public record LoginDTO(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record UpdateProfileDTO(
        [MaxLength(150)] string? FullName,
        [MaxLength(150)] string? AvatarUrl,
        [MaxLength(8)] string? Currency
    );

    public record ChangePasswordDTO(
        [Required] string OldPassword,
        [Required, MinLength(6)] string NewPassword
    );

    public record AuthResponseDTO(
        int UserId,
        string FullName,
        string Email,
        string Role,
        string Currency,
        string Token,
        DateTime ExpiresAt
    );

    public record UserResponseDTO(
        int UserId,
        string FullName,
        string Email,
        string Currency,
        string? AvatarUrl,
        string Role,
        bool isActive,
        string AuthProvider,
        DateTime CreatedAt,
        DateTime? LastLoginAt
    );
}