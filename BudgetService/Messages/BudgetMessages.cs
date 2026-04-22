namespace BudgetService.Messages
{
    // Must share namespace/type structure with ExpenseService.Messages.ExpenseCreatedEvent
    // in terms of property shape — MassTransit matches by message contract shape + namespace.
    public record ExpenseCreatedEvent(
        int ExpenseId,
        int UserId,
        int CategoryId,
        decimal Amount,
        string Currency,
        DateTime Date
    );

    // Published by BudgetService when a threshold is crossed; NotificationService consumes.
    public record BudgetAlertEvent(
        int UserId,
        int BudgetId,
        string BudgetName,
        string AlertType,    // WARNING | LIMIT_REACHED
        decimal LimitAmount,
        decimal SpentAmount,
        decimal UtilizationPercent,
        DateTime OccurredAt
    );
}