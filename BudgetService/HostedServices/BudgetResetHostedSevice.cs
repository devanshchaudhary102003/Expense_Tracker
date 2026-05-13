using BudgetService.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.HostedService
{
    // Runs every hour. On the 1st of each month (UTC), resets SpentAmount = 0 for all
    // active MONTHLY budgets and rolls the window forward by one month.

    public class BudgetResetHostedService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<BudgetResetHostedService> _logger;
        private DateTime _lastRunMonth = DateTime.MinValue;

        public BudgetResetHostedService(IServiceProvider sp, ILogger<BudgetResetHostedService> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    if (now.Day == 1 && (_lastRunMonth.Year != now.Year || _lastRunMonth.Month != now.Month))
                    {
                        await ResetAsync();
                        _lastRunMonth = new DateTime(now.Year, now.Month, 1);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Budget reset hosted service error");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ResetAsync()
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();

            var rows = await db.Budgets
                .Where(b => b.IsActive && b.Period == "MONTHLY")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.SpentAmount, _ => 0m)
                    .SetProperty(b => b.StartDate, b => b.StartDate.AddMonths(1))
                    .SetProperty(b => b.EndDate,   b => b.EndDate.AddMonths(1)));

            _logger.LogInformation("Monthly budget reset: {Rows} budgets rolled forward.", rows);
        }
    }
}