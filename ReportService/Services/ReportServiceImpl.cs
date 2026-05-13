using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.DTOs;
using ReportService.Interfaces;
using ReportService.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ReportService.Services
{
    public class ReportServiceImpl : IReportService
    {
        private readonly ReportDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<ReportServiceImpl> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public ReportServiceImpl(ReportDbContext db, IHttpClientFactory httpFactory, IConfiguration config, ILogger<ReportServiceImpl> logger)
        {
            _db = db;
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        // ---------- Analytics ----------

        public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int userId, int year, int month, string bearerToken)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end   = start.AddMonths(1).AddTicks(-1);

            var expenses  = await FetchExpensesInRangeAsync(start, end, bearerToken);
            var incomes   = await FetchIncomesInRangeAsync(start, end, bearerToken);
            var categories= await FetchCategoriesAsync(bearerToken);

            var totalExpense = expenses.Sum(e => e.Amount);
            var totalIncome  = incomes.Sum(i => i.Amount);
            var netBalance   = totalIncome - totalExpense;

            var topCategoryId = expenses
                .GroupBy(e => e.CategoryId)
                .OrderByDescending(g => g.Sum(x => x.Amount))
                .Select(g => (int?)g.Key).FirstOrDefault();

            string? topCategoryName = topCategoryId.HasValue
                ? categories.FirstOrDefault(c => c.CategoryId == topCategoryId.Value)?.Name
                : null;

            var savingsRate = totalIncome == 0 ? 0
                : Math.Round(((totalIncome - totalExpense) / totalIncome) * 100m, 2);

            return new MonthlySummaryDto(year, month, totalExpense, totalIncome, netBalance, topCategoryName, savingsRate);
        }

        public async Task<IEnumerable<CategoryBreakdownItem>> GetCategoryBreakdownAsync(int userId, DateTime start, DateTime end, string bearerToken)
        {
            var expenses = await FetchExpensesInRangeAsync(start, end, bearerToken);
            var categories = await FetchCategoriesAsync(bearerToken);
            var catMap = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            return expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoryBreakdownItem(
                    g.Key,
                    catMap.TryGetValue(g.Key, out var n) ? n : $"Category #{g.Key}",
                    g.Sum(x => x.Amount)))
                .OrderByDescending(x => x.TotalAmount)
                .ToList();
        }

        public async Task<IEnumerable<TrendPoint>> GetTrendAsync(int userId, int months, string bearerToken)
        {
            if (months < 1) months = 1;
            if (months > 24) months = 24;

            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));
            var end = now;

            var expenses = await FetchExpensesInRangeAsync(start, end, bearerToken);
            var incomes  = await FetchIncomesInRangeAsync(start, end, bearerToken);

            var points = new List<TrendPoint>();
            for (int i = 0; i < months; i++)
            {
                var monthStart = start.AddMonths(i);
                var monthEnd   = monthStart.AddMonths(1);

                var exp = expenses.Where(e => e.Date >= monthStart && e.Date < monthEnd).Sum(e => e.Amount);
                var inc = incomes .Where(e => e.Date >= monthStart && e.Date < monthEnd).Sum(e => e.Amount);

                points.Add(new TrendPoint($"{monthStart:yyyy-MM}", exp, inc));
            }
            return points;
        }

        public async Task<SavingsRateDto> GetSavingsRateAsync(int userId, int year, int month, string bearerToken)
        {
            var summary = await GetMonthlySummaryAsync(userId, year, month, bearerToken);
            return new SavingsRateDto(year, month, summary.SavingsRate, summary.TotalIncome, summary.TotalExpense);
        }

        public async Task<YearlySummaryDto> GetYearlySummaryAsync(int userId, int year, string bearerToken)
        {
            var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end   = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var expenses = await FetchExpensesInRangeAsync(start, end, bearerToken);
            var incomes  = await FetchIncomesInRangeAsync(start, end, bearerToken);

            var monthly = new List<TrendPoint>();
            for (int m = 1; m <= 12; m++)
            {
                var mStart = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
                var mEnd   = mStart.AddMonths(1);
                var exp = expenses.Where(e => e.Date >= mStart && e.Date < mEnd).Sum(e => e.Amount);
                var inc = incomes .Where(e => e.Date >= mStart && e.Date < mEnd).Sum(e => e.Amount);
                monthly.Add(new TrendPoint($"{year}-{m:D2}", exp, inc));
            }

            var totalExpense = expenses.Sum(e => e.Amount);
            var totalIncome  = incomes.Sum(i => i.Amount);
            return new YearlySummaryDto(year, totalExpense, totalIncome, totalIncome - totalExpense, monthly);
        }

        public async Task<IEnumerable<CategoryBreakdownItem>> GetTopCategoriesAsync(int userId, int limit, string bearerToken)
        {
            if (limit < 1) limit = 5;
            var end = DateTime.UtcNow;
            var start = end.AddMonths(-12);
            var all = await GetCategoryBreakdownAsync(userId, start, end, bearerToken);
            return all.Take(limit);
        }

        // ---------- Report metadata persistence ----------

        public async Task<ReportResponseDto> SaveReportMetadataAsync(int userId, string reportType, string title, string? filePath, string? parameters)
        {
        var r = new ReportEntity
        {
            UserId = userId,
            ReportType = reportType.ToUpperInvariant(),
            Title = title,
            FilePath = filePath,
            Parameters = parameters,
            Status = "GENERATED"
        };
        _db.Reports.Add(r);
        await _db.SaveChangesAsync();
        return ToResponse(r);
    }

        public async Task<IEnumerable<ReportResponseDto>> GetReportsByUserAsync(int userId) =>
            (await _db.Reports.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GeneratedAt).ToListAsync()).Select(ToResponse);

        public async Task DeleteReportAsync(int reportId, int userId)
        {
            var rows = await _db.Reports.Where(r => r.ReportId == reportId && r.UserId == userId).ExecuteDeleteAsync();
            if (rows == 0) throw new KeyNotFoundException($"Report {reportId} not found.");
        }

        // ---------- Helpers: cross-service HTTP ----------

        private HttpClient Client(string token)
        {
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }

        private string ExpenseBase  => _config["Services:Expense"]  ?? "http://localhost:5002";
        private string IncomeBase   => _config["Services:Income"]   ?? "http://localhost:5003";
        private string CategoryBase => _config["Services:Category"] ?? "http://localhost:5004";

        private async Task<List<ExpenseDto>> FetchExpensesInRangeAsync(DateTime start, DateTime end, string token)
        {
            try
            {
                var url = $"{ExpenseBase}/api/expenses/filter?start={start:o}&end={end:o}";
                var resp = await Client(token).GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Expense service returned {Status} for {Url}", resp.StatusCode, url);
                    return new();
                }
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ExpenseDto>>(json, JsonOpts) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FetchExpenses failed");
                return new();
            }
        }

        private async Task<List<IncomeDto>> FetchIncomesInRangeAsync(DateTime start, DateTime end, string token)
        {
            try
            {
                var url = $"{IncomeBase}/api/incomes/filter?start={start:o}&end={end:o}";
                var resp = await Client(token).GetAsync(url);
                if (!resp.IsSuccessStatusCode) return new();
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<IncomeDto>>(json, JsonOpts) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FetchIncomes failed");
                return new();
            }
        }

        private async Task<List<CategoryDto>> FetchCategoriesAsync(string token)
        {
            try
            {
                var resp = await Client(token).GetAsync($"{CategoryBase}/api/categories");
                if (!resp.IsSuccessStatusCode) return new();
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<CategoryDto>>(json, JsonOpts) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FetchCategories failed");
                return new();
            }
        }

        private static ReportResponseDto ToResponse(ReportEntity r) => new(
            r.ReportId, 
            r.UserId, 
            r.ReportType, 
            r.Title,
            r.GeneratedAt, 
            r.FilePath, 
            r.Parameters, 
            r.Status
        );
    }
}