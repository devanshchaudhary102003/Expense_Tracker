using BudgetService.DTOs;

namespace BudgetService.Interfaces
{
    public interface IBudgetService
    {
        Task<BudgetResponseDto> CreateAsync(int userId, CreateBudgetDto dto);
        Task<BudgetResponseDto> GetByIdAsync(int id, int userId);
        Task<IEnumerable<BudgetResponseDto>> GetByUserAsync(int userId);
        Task<IEnumerable<BudgetResponseDto>> GetActiveAsync(int userId);
        Task<IEnumerable<BudgetResponseDto>> GetOverBudgetAsync(int userId);
        Task<BudgetResponseDto> UpdateAsync(int id, int userId, UpdateBudgetDto dto);
        Task DeleteAsync(int id, int userId);
        Task<decimal> GetUtilizationAsync(int userId);

        // Called by MassTransit consumer, atomic update + conditional alert publish.
        Task CheckBudgetOnExpenseAsync(int userId, int categoryId, decimal amount, DateTime occurredAt);
    }

}