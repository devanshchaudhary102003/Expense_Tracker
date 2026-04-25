using MassTransit;
using NotificationService.Interfaces;
using NotificationService.Messages;

namespace NotificationService.Consumers
{
    public class BudgetAlertConsumer : IConsumer<BudgetAlertEvent>
    {
        private readonly INotificationService _notif;
        private readonly ILogger<BudgetAlertConsumer> _logger;

        public BudgetAlertConsumer(INotificationService notif, ILogger<BudgetAlertConsumer> logger)
        {
            _notif = notif;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BudgetAlertEvent> context)
        {
            var m = context.Message;
            _logger.LogInformation("BudgetAlert consumed: user={UserId} budget={BudgetId} type={Type}",
                m.UserId, m.BudgetId, m.AlertType);

            await _notif.SendBudgetAlertAsync(
                m.UserId, 
                m.BudgetId, 
                m.BudgetName,
                m.AlertType, 
                m.LimitAmount, 
                m.SpentAmount, 
                m.UtilizationPercent);
        }
    }
}