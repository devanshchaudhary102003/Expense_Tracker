using System.ComponentModel.DataAnnotations;

namespace BudgetService.DTOs
{
    public record CreateBudgetDto(
        [Required, MaxLength(150)] 
        string Name,

        int? CategoryId,

        [Required, Range(0.01, double.MaxValue)]
         decimal LimitAmount,

        string? Currency,

        [Required] 
        string Period,

        [Required]
         DateTime StartDate,

        [Required]
         DateTime EndDate
    );

    public record UpdateBudgetDto(
        string? Name,
        int? CategoryId,
        decimal? LimitAmount,
        string? Currency,
        string? Period,
        DateTime? StartDate,
        DateTime? EndDate,
        bool? IsActive
    );

    public record BudgetResponseDto(
        int BudgetId, 
        int UserId, 
        int? CategoryId, 
        string Name,
        decimal LimitAmount,
        decimal SpentAmount, 
        decimal RemainingAmount, 
        decimal UtilizationPercent,
        string Currency,
        string Period, 
        DateTime StartDate, 
        DateTime EndDate,
        bool IsActive, 
        DateTime CreatedAt
    );
}