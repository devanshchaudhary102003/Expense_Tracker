using System.ComponentModel.DataAnnotations;

namespace BudgetService.Models
{
    public class BudgetEntity
    {
        [Key]
        public int BudgetId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; } // null = overall budget

        [Required, MaxLength(150)] 
        public string Name { get; set; } = string.Empty;

        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }

        [MaxLength(8)]
        public string Currency { get; set; } = "INR";
        [MaxLength(20)] 
        public string Period   { get; set; } = "MONTHLY"; // MONTHLY | WEEKLY | CUSTOM

        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}