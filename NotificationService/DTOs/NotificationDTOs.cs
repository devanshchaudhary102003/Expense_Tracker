using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    public record CreateNotificationDto(
        [Required]
        int UserId,

        [Required]
        string Type,

        [Required]
        string Title,

        [Required]
        string Message,

        int? RelatedId
    );

    public record BroadcastNotificationDto(
        [Required]
        string Title,

        [Required]
        string Message,

        IList<int>? UserIds     // null = all users (admin must supply)
    );

    public record NotificationResponseDto(
        int NotificationId,
        int UserId,
        string Type,
        string Title,
        string Message,
        int? RelatedId,
        bool IsRead,
        DateTime SentAt
    );

    public record UnreadCountDto(int Count);
}