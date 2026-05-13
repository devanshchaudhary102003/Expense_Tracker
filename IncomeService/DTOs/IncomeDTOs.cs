using System.ComponentModel.DataAnnotations;

namespace IncomeService.DTOs
{
    public record CreateIncomeDto(
        [Required] string Source,
        [Required, Range(0.01, double.MaxValue)] decimal Amount,
        string? Currency,
        string? Description,
        DateTime? Date,
        bool IsRecurring,
        string? RecurrenceType
    );

    public record UpdateIncomeDto(
        string? Source,
        decimal? Amount,
        string? Currency,
        string? Description,
        DateTime? Date,
        bool? IsRecurring,
        string? RecurrenceType
    );

    public record IncomeResponseDto(
        int IncomeId, int UserId, string Source, decimal Amount, string Currency,
        string? Description, DateTime Date, bool IsRecurring, string? RecurrenceType,
        DateTime CreatedAt, DateTime? UpdatedAt
    );

    public record NetBalanceDto(decimal TotalIncome, decimal TotalExpense, decimal NetBalance);

}