using IncomeService.Data;
using IncomeService.DTOs;
using IncomeService.Interfaces;
using IncomeService.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace IncomeService.Services;

public class IncomeServiceImpl : IIncomeService
{
    private readonly IncomeDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<IncomeServiceImpl> _logger;

    public IncomeServiceImpl(IncomeDbContext db, IHttpClientFactory httpFactory,
        IConfiguration config, ILogger<IncomeServiceImpl> logger)
    {
        _db = db;
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<IncomeResponseDto> AddAsync(int userId, CreateIncomeDto dto)
    {
        var income = new IncomeEntity
        {
            UserId = userId,
            Source = dto.Source.ToUpperInvariant(),
            Amount = dto.Amount,
            Currency = (dto.Currency ?? "INR").ToUpperInvariant(),
            Description = dto.Description,
            Date = dto.Date ?? DateTime.UtcNow,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.RecurrenceType?.ToUpperInvariant()
        };
        _db.Incomes.Add(income);
        await _db.SaveChangesAsync();
        return ToResponse(income);
    }

    public async Task<IncomeResponseDto> GetByIdAsync(int id, int userId)
    {
        var i = await _db.Incomes.FirstOrDefaultAsync(x => x.IncomeId == id && x.UserId == userId)
            ?? throw new KeyNotFoundException($"Income {id} not found.");
        return ToResponse(i);
    }

    public async Task<IEnumerable<IncomeResponseDto>> GetByUserAsync(int userId) =>
        (await _db.Incomes.AsNoTracking().Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date).ToListAsync()).Select(ToResponse);

    public async Task<IEnumerable<IncomeResponseDto>> GetBySourceAsync(int userId, string source) =>
        (await _db.Incomes.AsNoTracking()
            .Where(e => e.UserId == userId && e.Source == source.ToUpper())
            .OrderByDescending(e => e.Date).ToListAsync()).Select(ToResponse);

    public async Task<IEnumerable<IncomeResponseDto>> GetByDateRangeAsync(int userId, DateTime start, DateTime end) =>
        (await _db.Incomes.AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
            .OrderByDescending(e => e.Date).ToListAsync()).Select(ToResponse);

    public async Task<IEnumerable<IncomeResponseDto>> GetRecurringAsync(int userId) =>
        (await _db.Incomes.AsNoTracking().Where(e => e.UserId == userId && e.IsRecurring)
            .OrderByDescending(e => e.Date).ToListAsync()).Select(ToResponse);

    public async Task<decimal> GetTotalByUserAsync(int userId) =>
        await _db.Incomes.Where(e => e.UserId == userId).SumAsync(e => (decimal?)e.Amount) ?? 0m;

    public async Task<decimal> GetTotalBySourceAsync(int userId, string source) =>
        await _db.Incomes.Where(e => e.UserId == userId && e.Source == source.ToUpper())
            .SumAsync(e => (decimal?)e.Amount) ?? 0m;

    public async Task<NetBalanceDto> GetNetBalanceAsync(int userId, string bearerToken)
    {
        var totalIncome = await GetTotalByUserAsync(userId);

        var expenseBase = _config["Services:Expense"] ?? "http://localhost:5002";
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        decimal totalExpense = 0m;
        try
        {
            var resp = await client.GetAsync($"{expenseBase}/api/expenses/total");
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                decimal.TryParse(body, out totalExpense);
            }
            else _logger.LogWarning("Expense service returned {Status}", resp.StatusCode);
        }
        catch (Exception ex) { _logger.LogError(ex, "Could not reach ExpenseService for net balance."); }

        return new NetBalanceDto(totalIncome, totalExpense, totalIncome - totalExpense);
    }

    public async Task<IncomeResponseDto> UpdateAsync(int id, int userId, UpdateIncomeDto dto)
    {
        var i = await _db.Incomes.FirstOrDefaultAsync(x => x.IncomeId == id && x.UserId == userId)
            ?? throw new KeyNotFoundException($"Income {id} not found.");

        if (dto.Source != null)         i.Source         = dto.Source.ToUpperInvariant();
        if (dto.Amount.HasValue)        i.Amount         = dto.Amount.Value;
        if (dto.Currency != null)       i.Currency       = dto.Currency.ToUpperInvariant();
        if (dto.Description != null)    i.Description    = dto.Description;
        if (dto.Date.HasValue)          i.Date           = dto.Date.Value;
        if (dto.IsRecurring.HasValue)   i.IsRecurring    = dto.IsRecurring.Value;
        if (dto.RecurrenceType != null) i.RecurrenceType = dto.RecurrenceType.ToUpperInvariant();
        i.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToResponse(i);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var rows = await _db.Incomes.Where(x => x.IncomeId == id && x.UserId == userId).ExecuteDeleteAsync();
        if (rows == 0) throw new KeyNotFoundException($"Income {id} not found.");
    }

    private static IncomeResponseDto ToResponse(IncomeEntity i) => new
    (
        i.IncomeId, 
        i.UserId, 
        i.Source, 
        i.Amount, 
        i.Currency, 
        i.Description,
        i.Date, 
        i.IsRecurring, 
        i.RecurrenceType, 
        i.CreatedAt, 
        i.UpdatedAt
    );
}
