namespace ExpenseService.Messages
{
    // Published by ExpenseService after a new expense is saved.
    // Consumed by BudgetService.CheckBudgetOnExpense().

    public record ExpenseCreatedEvent
    (
        int ExpenseId,
        int UserId,
        int CategoryId,
        decimal Amount,
        string Currency,
        DateTime Date

    );
}