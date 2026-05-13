using CategoryService.DTOs;

namespace CategoryService.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateAsync(int userId, CreateCategoryDto dto);
        Task<CategoryResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<CategoryResponseDto>> GetDefaultsAsync();
        Task<IEnumerable<CategoryResponseDto>> GetAllForUserAsync(int userId);
        Task<IEnumerable<CategoryResponseDto>> GetByTypeAsync(int userId, string type);
        Task<CategoryResponseDto> UpdateAsync(int id, int userId, UpdateCategoryDto dto);
        Task DeactivateAsync(int id, int userId);
        Task DeleteAsync(int id, int userId);
    }
}