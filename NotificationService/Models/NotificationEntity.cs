using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class NotificationEntity
    {
        [Key]
        public int NotificationId { get; set; }
        public int UserId { get ;set; }
        [MaxLength(40)]
        public string Type { get; set; } = "PLATFORM";  // BUDGET_WARNING | BUDGET_EXCEEDED | MONTHLY_SUMMARY | RECURRING_REMINDER | PLATFORM

        [Required,MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required,MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public int? RelatedId { get; set; }     // budgetId / expenseId

        public bool IsRead { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}