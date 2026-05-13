using ReportService.DTOs;

namespace ReportService.Interfaces
{
    public interface IReportService
    {
        Task<MonthlySummaryDto> GetMonthlySummaryAsync(int userId, int year, int month, string bearerToken);
        Task<IEnumerable<CategoryBreakdownItem>> GetCategoryBreakdownAsync(int userId, DateTime start, DateTime end, string bearerToken);
        Task<IEnumerable<TrendPoint>> GetTrendAsync(int userId, int months, string bearerToken);
        Task<SavingsRateDto> GetSavingsRateAsync(int userId, int year, int month, string bearerToken);
        Task<YearlySummaryDto> GetYearlySummaryAsync(int userId, int year, string bearerToken);
        Task<IEnumerable<CategoryBreakdownItem>> GetTopCategoriesAsync(int userId, int limit, string bearerToken);

        Task<ReportResponseDto> SaveReportMetadataAsync(int userId, string reportType, string title, string? filePath, string? parameters);
        Task<IEnumerable<ReportResponseDto>> GetReportsByUserAsync(int userId);
        Task DeleteReportAsync(int reportId, int userId);
    }
}