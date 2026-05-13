using ExpenseService.DTOs;

namespace ExpenseService.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseResponseDTO> AddExpenseAsync(int userId, CreateExpenseDTO dto);
        Task<ExpenseResponseDTO> GetByIdAsync(int expenseId, int userId);
        Task<IEnumerable<ExpenseResponseDTO>> GetByUserAsync(int userId);
        Task<IEnumerable<ExpenseResponseDTO>> GetByCategoryAsync(int userId, int categoryId);
        Task<IEnumerable<ExpenseResponseDTO>> GetByDateRangeAsync(int userId, DateTime start, DateTime end);
        Task<IEnumerable<ExpenseResponseDTO>> GetByPaymentModeAsync(int userId, string mode);
        Task<IEnumerable<ExpenseResponseDTO>> SearchAsync(int userId, string mode);
        Task<IEnumerable<ExpenseResponseDTO>> GetRecurringAsync(int userId);
        Task<decimal> GetTotalByUserAsync(int userId);
        Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId);
        Task<ExpenseResponseDTO> UpdateAsync(int expenseId, int userId, UpdateExpenseDTO dto);
        Task DeleteAsync(int expenseId, int userId);
    }
}