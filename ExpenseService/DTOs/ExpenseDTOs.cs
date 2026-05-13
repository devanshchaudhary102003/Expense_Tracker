using System.ComponentModel.DataAnnotations;

namespace ExpenseService.DTOs
{
    public record CreateExpenseDTO(
        [Required]
        int CategoryId,
        [Required,Range(0.01,double.MaxValue)] decimal Amount,
        string? Currency,
        string? Description,
        DateTime? Date,
        string? PaymentMode,
        string? ReceiptUrl,
        string? Tags,
        bool IsRecurring
    );

    public record UpdateExpenseDTO(
        int? CategoryId,
        decimal? Amount,
        string? Currency,
        string? Description,
        DateTime? Date,
        string? PaymentMode,
        string? ReceiptUrl,
        string? Tags,
        bool? IsRecurring
    );

    public record ExpenseResponseDTO(
        int ExpenseId,
        int UserId,
        int CategoryId,
        decimal Amount,
        string Currency,
        string? Description,
        DateTime Date,
        string PaymentMode,
        string? ReceiptUrl,
        string? Tags,
        bool IsRecurring,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}