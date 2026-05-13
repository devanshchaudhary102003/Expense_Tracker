using System.ComponentModel.DataAnnotations;

namespace ExpenseService.Models
{
    public class ExpenseEntity
    {
        [Key]
        public int ExpenseId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }

        [MaxLength(8)]
        public string Currency { get; set; } = "INR";

        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string PaymentMode { get; set; } = "CASH";// CASH | CARD | UPI | NET_BANKING | WALLET
        [MaxLength(1000)]
        public string? ReceiptUrl { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        public bool IsRecurring { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}