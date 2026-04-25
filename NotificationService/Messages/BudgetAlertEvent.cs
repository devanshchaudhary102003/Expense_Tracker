namespace NotificationService.Messages
{
    // Must match the shape of BudgetService.Messages.BudgetAlertEvent for MassTransit routing.
    public record BudgetAlertEvent(
        int UserId,
        int BudgetId,
        string BudgetName,
        string AlertType,   // WARNING | LIMIT_REACHED
        decimal LimitAmount,
        decimal SpentAmount,
        decimal UtilizationPercent,
        DateTime OccurredAt
    );
}