using IncomeService.DTOs;

namespace IncomeService.Interfaces
{
    public interface IIncomeService
    {
        Task<IncomeResponseDto> AddAsync(int userId, CreateIncomeDto dto);
        Task<IncomeResponseDto> GetByIdAsync(int id, int userId);
        Task<IEnumerable<IncomeResponseDto>> GetByUserAsync(int userId);
        Task<IEnumerable<IncomeResponseDto>> GetBySourceAsync(int userId, string source);
        Task<IEnumerable<IncomeResponseDto>> GetByDateRangeAsync(int userId, DateTime start, DateTime end);
        Task<IEnumerable<IncomeResponseDto>> GetRecurringAsync(int userId);
        Task<decimal> GetTotalByUserAsync(int userId);
        Task<decimal> GetTotalBySourceAsync(int userId, string source);
        Task<NetBalanceDto> GetNetBalanceAsync(int userId, string bearerToken);
        Task<IncomeResponseDto> UpdateAsync(int id, int userId, UpdateIncomeDto dto);
        Task DeleteAsync(int id, int userId);
    }
}