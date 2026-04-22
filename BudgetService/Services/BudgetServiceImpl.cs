using BudgetService.Data;
using BudgetService.DTOs;
using BudgetService.Interfaces;
using BudgetService.Messages;
using BudgetService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.Services
{
    public class BudgetServiceImpl : IBudgetService
    {
        private readonly BudgetDbContext _db;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<BudgetServiceImpl> _logger;

        public BudgetServiceImpl(BudgetDbContext db, IPublishEndpoint publisher, ILogger<BudgetServiceImpl> logger)
        {
            _db = db;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<BudgetResponseDto> CreateAsync(int userId, CreateBudgetDto dto)
        {
            var period = dto.Period.ToUpperInvariant();
            if (period is not ("MONTHLY" or "WEEKLY" or "CUSTOM"))
                throw new InvalidOperationException("Period must be MONTHLY, WEEKLY, or CUSTOM.");
            if (dto.EndDate <= dto.StartDate)
                throw new InvalidOperationException("EndDate must be after StartDate.");

            var b = new BudgetEntity
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Name = dto.Name.Trim(),
                LimitAmount = dto.LimitAmount,
                SpentAmount = 0,
                Currency = (dto.Currency ?? "INR").ToUpperInvariant(),
                Period = period,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            _db.Budgets.Add(b);
            await _db.SaveChangesAsync();
            return ToResponse(b);
        }

        public async Task<BudgetResponseDto> GetByIdAsync(int id, int userId)
        {
            var b = await _db.Budgets.FirstOrDefaultAsync(x => x.BudgetId == id && x.UserId == userId)
                ?? throw new KeyNotFoundException($"Budget {id} not found.");
            return ToResponse(b);
        }

        public async Task<IEnumerable<BudgetResponseDto>> GetByUserAsync(int userId) =>
            (await _db.Budgets.AsNoTracking().Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt).ToListAsync()).Select(ToResponse);

        public async Task<IEnumerable<BudgetResponseDto>> GetActiveAsync(int userId) =>
            (await _db.Budgets.AsNoTracking()
                .Where(b => b.UserId == userId && b.IsActive
                        && b.StartDate <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow)
                .ToListAsync()).Select(ToResponse);

        public async Task<IEnumerable<BudgetResponseDto>> GetOverBudgetAsync(int userId) =>
            (await _db.Budgets.AsNoTracking()
                .Where(b => b.UserId == userId && b.IsActive && b.SpentAmount > b.LimitAmount)
                .ToListAsync()).Select(ToResponse); 

        public async Task<BudgetResponseDto> UpdateAsync(int id, int userId, UpdateBudgetDto dto)
        {
            var b = await _db.Budgets.FirstOrDefaultAsync(x => x.BudgetId == id && x.UserId == userId)
                ?? throw new KeyNotFoundException($"Budget {id} not found.");

            if (dto.Name != null)           b.Name        = dto.Name.Trim();
            if (dto.CategoryId.HasValue)    b.CategoryId  = dto.CategoryId;
            if (dto.LimitAmount.HasValue)   b.LimitAmount = dto.LimitAmount.Value;
            if (dto.Currency != null)       b.Currency    = dto.Currency.ToUpperInvariant();
            if (dto.Period != null)         b.Period      = dto.Period.ToUpperInvariant();
            if (dto.StartDate.HasValue)     b.StartDate   = dto.StartDate.Value;
            if (dto.EndDate.HasValue)       b.EndDate     = dto.EndDate.Value;
            if (dto.IsActive.HasValue)      b.IsActive    = dto.IsActive.Value;

            await _db.SaveChangesAsync();
            return ToResponse(b);
        }

        public async Task DeleteAsync(int id, int userId)
        {
        var rows = await _db.Budgets.Where(b => b.BudgetId == id && b.UserId == userId).ExecuteDeleteAsync();
        if (rows == 0) throw new KeyNotFoundException($"Budget {id} not found.");
        }

        public async Task<decimal> GetUtilizationAsync(int userId)
        {
            var totals = await _db.Budgets.AsNoTracking()
                .Where(b => b.UserId == userId && b.IsActive)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Limit = g.Sum(x => x.LimitAmount),
                    Spent = g.Sum(x => x.SpentAmount)
                })
                .FirstOrDefaultAsync();

            if (totals == null || totals.Limit == 0) return 0m;
            return Math.Round((totals.Spent / totals.Limit) * 100m, 2);
        }

        public async Task CheckBudgetOnExpenseAsync(int userId, int categoryId, decimal amount, DateTime occurredAt)
        {
            // Match budgets: category-specific OR overall (CategoryId == null), active, in date window.
            var candidates = await _db.Budgets
                .Where(b => b.UserId == userId && b.IsActive
                        && (b.CategoryId == null || b.CategoryId == categoryId)
                        && b.StartDate <= occurredAt && b.EndDate >= occurredAt)
                .ToListAsync();

            foreach (var budget in candidates)
            {
                // Atomic increment — prevents race conditions if many expenses land at once.
                await _db.Budgets.Where(b => b.BudgetId == budget.BudgetId)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.SpentAmount, x => x.SpentAmount + amount));

                var newSpent = budget.SpentAmount + amount;
                var util = budget.LimitAmount == 0 ? 0 : (newSpent / budget.LimitAmount) * 100m;

                string? alertType = null;
                if (newSpent >= budget.LimitAmount) alertType = "LIMIT_REACHED";
                else if (util >= 80m)               alertType = "WARNING";

                if (alertType != null)
                {
                    await _publisher.Publish(new BudgetAlertEvent(
                        budget.UserId, budget.BudgetId, budget.Name, alertType,
                        budget.LimitAmount, newSpent, Math.Round(util, 2), DateTime.UtcNow));

                    _logger.LogInformation(
                        "Budget {Id} alert {Type} for user {UserId} ({Util}%)",
                        budget.BudgetId, alertType, budget.UserId, Math.Round(util, 2));
                }
            }
        }

        private static BudgetResponseDto ToResponse(BudgetEntity b)
        {
            var remaining = b.LimitAmount - b.SpentAmount;
            var util = b.LimitAmount == 0 ? 0 : Math.Round((b.SpentAmount / b.LimitAmount) * 100m, 2);
            return new BudgetResponseDto(
                b.BudgetId, b.UserId, b.CategoryId, b.Name,
                b.LimitAmount, b.SpentAmount, remaining, util,
                b.Currency, b.Period, b.StartDate, b.EndDate,
                b.IsActive, b.CreatedAt);
        }
    }    
}