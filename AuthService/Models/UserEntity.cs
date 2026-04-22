using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models;

[Table("User")]
public class UserEntity
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    // Nullable because OAuth (Google/GitHub) users may not have a local password.
    public string? PasswordHash { get; set; }

    // ISO 4217 currency code — INR, USD, GBP, etc.
    [MaxLength(8)]
    public string Currency { get; set; } = "INR";

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(30)]
    public string Role { get; set; } = "User"; // User or Admin

    public bool IsActive { get; set; } = true;

    // OAuth provider metadata
    [MaxLength(30)]
    public string AuthProvider { get; set; } = "Local"; // Local | Google | GitHub

    [MaxLength(200)]
    public string? ExternalId { get; set; } // sub (Google) / id (GitHub)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
