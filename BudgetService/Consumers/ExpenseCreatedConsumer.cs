using BudgetService.Interfaces;
using BudgetService.Messages;
using MassTransit;

namespace BudgetService.Consumers
{
    public class ExpenseCreatedConsumer : IConsumer<ExpenseCreatedEvent>
    {
        private readonly IBudgetService _budget;
        private readonly ILogger<ExpenseCreatedConsumer> _logger;

        public ExpenseCreatedConsumer(IBudgetService budget, ILogger<ExpenseCreatedConsumer> logger)
        {
            _budget = budget;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ExpenseCreatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("ExpenseCreated consumed: expense={ExpenseId} user={UserId} amt={Amount}",
                msg.ExpenseId, msg.UserId, msg.Amount);

            await _budget.CheckBudgetOnExpenseAsync(msg.UserId, msg.CategoryId, msg.Amount, msg.Date);
        }
    }
}