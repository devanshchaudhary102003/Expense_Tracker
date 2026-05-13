namespace ReportService.DTOs
{
    public record MonthlySummaryDto(
        int Year, int Month,
        decimal TotalExpense, decimal TotalIncome, decimal NetBalance,
        string? TopCategory, decimal SavingsRate
    );

    public record CategoryBreakdownItem(
        int CategoryId, 
        string CategoryName, 
        decimal TotalAmount
    );

    public record TrendPoint(
        string MonthYear, 
        decimal TotalExpense, 
        decimal TotalIncome
    );

    public record SavingsRateDto(
        int Year, 
        int Month, 
        decimal SavingsRate, 
        decimal TotalIncome, 
        decimal TotalExpense
    );

    public record YearlySummaryDto(
        int Year, 
        decimal TotalExpense, 
        decimal TotalIncome, 
        decimal NetBalance, 
        IList<TrendPoint> Monthly
    );

    public record ReportResponseDto(
        int ReportId, 
        int UserId, 
        string ReportType, 
        string Title,
        DateTime GeneratedAt, 
        string? FilePath, 
        string? Parameters, 
        string Status
    );

    // Internal shapes for deserialising cross-service responses.
    public record ExpenseDto(
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

    public record IncomeDto(
        int IncomeId, 
        int UserId, 
        string Source, 
        decimal Amount, 
        string Currency,
        string? Description, 
        DateTime Date, 
        bool IsRecurring, 
        string? RecurrenceType,
        DateTime CreatedAt, 
        DateTime? UpdatedAt
    );

    public record CategoryDto(
        int CategoryId, 
        int? UserId, 
        string Name, 
        string? Icon, 
        string? Color,
        string Type, 
        bool IsDefault, 
        bool IsActive, 
        DateTime CreatedAt
    );
}