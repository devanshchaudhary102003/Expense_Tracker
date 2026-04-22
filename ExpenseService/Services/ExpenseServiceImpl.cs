using ExpenseService.Data;
using ExpenseService.DTOs;
using ExpenseService.Interfaces;
using ExpenseService.Messages;
using ExpenseService.Models;
using MassTransit;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Services
{
    public class ExpenseServiceImpl : IExpenseService
    {
        private readonly ExpenseDbContext _db;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<ExpenseServiceImpl> _logger;

        public ExpenseServiceImpl(ExpenseDbContext db, IPublishEndpoint publisher, ILogger<ExpenseServiceImpl> logger)
        {
            _db = db;
            _publisher = publisher;
            _logger = logger;
        }       

        public async Task<ExpenseResponseDTO> AddExpenseAsync(int userId, CreateExpenseDTO dto)
        {
            var expense = new ExpenseEntity
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Currency = (dto.Currency ?? "INR").ToUpperInvariant(),
                Description = dto.Description,
                Date = dto.Date ?? DateTime.UtcNow,
                PaymentMode = (dto.PaymentMode ?? "CASH").ToUpperInvariant(),
                ReceiptUrl = dto.ReceiptUrl,
                Tags = dto.Tags,
                IsRecurring = dto.IsRecurring
            };

            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();

            // Fire-and-forget event — BudgetService consumer will update SpentAmount and fire alerts.
            await _publisher.Publish(new ExpenseCreatedEvent(
                expense.ExpenseId,
                expense.UserId,
                expense.CategoryId,
                expense.Amount,
                expense.Currency,
                expense.Date
            ));

            _logger.LogInformation("Expense {Id} created for {userId}; event published.",expense.ExpenseId, expense.UserId);

            return ToResponse(expense);
        }

        public async Task<ExpenseResponseDTO> GetByIdAsync(int expenseId, int userId)
        {
            var e = await _db.Expenses.FirstOrDefaultAsync(x => x.ExpenseId == expenseId && x.UserId == userId)
                    ?? throw new KeyNotFoundException($"Expense {expenseId} not found.");

            return ToResponse(e);
        }

        public async Task<IEnumerable<ExpenseResponseDTO>> GetByUserAsync(int userId) =>
            (await _db.Expenses.AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date).ToListAsync())
                .Select(ToResponse);

        public async Task<IEnumerable<ExpenseResponseDTO>> GetByCategoryAsync(int userId, int categoryId) =>
            (await _db.Expenses.AsNoTracking()
            .Where(e => e.UserId == userId && e.CategoryId == categoryId)
            .OrderByDescending(e => e.Date).ToListAsync())
            .Select(ToResponse);

        public async Task<IEnumerable<ExpenseResponseDTO>> GetByDateRangeAsync(int userId, DateTime start, DateTime end) =>
            (await _db.Expenses.AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
            .OrderByDescending(e => e.Date).ToListAsync())
            .Select(ToResponse);

        public async Task<IEnumerable<ExpenseResponseDTO>> GetByPaymentModeAsync(int userId, string mode) => 
            (await _db.Expenses.AsNoTracking()
                .Where(e => e.UserId == userId && e.PaymentMode == mode.ToUpper())
                .OrderByDescending(e => e.Date).ToListAsync())
                .Select(ToResponse);

        public async Task<IEnumerable<ExpenseResponseDTO>> SearchAsync(int userId, string keyword)
        {
            var pattern = $"%{keyword}%";
            return (await _db.Expenses.AsNoTracking()
                .Where(e => e.UserId == userId && e.Description != null && EF.Functions.Like(e.Description, pattern))
                .OrderByDescending(e => e.Date).ToListAsync())
                .Select(ToResponse);
        }

        public async Task<IEnumerable<ExpenseResponseDTO>> GetRecurringAsync(int userId) =>
            (await _db.Expenses.AsNoTracking()
                .Where(e => e.UserId == userId && e.IsRecurring)
                .OrderByDescending(e => e.Date).ToListAsync())
                .Select(ToResponse);

        public async Task<decimal> GetTotalByUserAsync(int userId) =>
            await _db.Expenses.Where(e => e.UserId == userId).SumAsync(e => (decimal?)e.Amount) ?? 0m;

        public async Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId) =>
            await _db.Expenses.Where(e => e.UserId == userId && e.CategoryId == categoryId).SumAsync(e => (decimal?)e.Amount) ?? 0m;

        public async Task<ExpenseResponseDTO> UpdateAsync(int userId, int expenseId, UpdateExpenseDTO dto)
        {
            var e = await _db.Expenses.FirstOrDefaultAsync(x => x.UserId == userId && x.ExpenseId == expenseId)
                    ?? throw new KeyNotFoundException($"Expense {expenseId} npt found.");

            if(dto.CategoryId.HasValue)     e.CategoryId = dto.CategoryId.Value;
            if(dto.Amount.HasValue)         e.Amount = dto.Amount.Value;
            if(dto.Currency != null)        e.Currency = dto.Currency.ToUpperInvariant();
            if(dto.Description != null)     e.Description = dto.Description;
            if(dto.Date.HasValue)           e.Date = dto.Date.Value;
            if(dto.PaymentMode != null)     e.PaymentMode = dto.PaymentMode.ToUpperInvariant();
            if(dto.ReceiptUrl != null)      e.ReceiptUrl = dto.ReceiptUrl;
            if(dto.Tags != null)            e.Tags = dto.Tags;
            if(dto.IsRecurring.HasValue)    e.IsRecurring = dto.IsRecurring.Value;
            e.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ToResponse(e); 
        }

        public async Task DeleteAsync(int expenseId, int userId)
        {
            var rows = await _db.Expenses
                            .Where(e => e.ExpenseId == expenseId && e.UserId == userId)
                            .ExecuteDeleteAsync();
                        
            if(rows == 0)   throw new KeyNotFoundException($"Expense {expenseId} npt found.");
        }
        private static ExpenseResponseDTO ToResponse(ExpenseEntity e) => new(
            e.ExpenseId,
            e.UserId,
            e.CategoryId,
            e.Amount,
            e.Currency,
            e.Description,
            e.Date,
            e.PaymentMode,
            e.ReceiptUrl,
            e.Tags,
            e.IsRecurring,
            e.CreatedAt,
            e.UpdatedAt
        );
    }
}