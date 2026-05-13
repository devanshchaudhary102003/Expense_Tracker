using System.ComponentModel.DataAnnotations;

namespace IncomeService.Models
{
    public class IncomeEntity
    {
        [Key] 
        public int IncomeId { get; set; }
        public int UserId { get; set; }

        [MaxLength(30)]
        public string Source { get; set; } = "OTHER"; // SALARY | FREELANCE | INVESTMENT | RENTAL | OTHER

        public decimal Amount { get; set; }
        [MaxLength(8)] 
        public string Currency { get; set; } = "INR";
        [MaxLength(500)]
         public string? Description { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public bool IsRecurring { get; set; }
        [MaxLength(20)] 
        public string? RecurrenceType { get; set; } // MONTHLY | WEEKLY | YEARLY

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

}