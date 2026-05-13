using CategoryService.Data;
using CategoryService.DTOs;
using CategoryService.Interfaces;
using CategoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Services
{
    public class CategoryServiceImpl : ICategoryService
    {
        private readonly CategoryDbContext _db;
        public CategoryServiceImpl(CategoryDbContext db) => _db = db;

        public async Task<CategoryResponseDto> CreateAsync(int userId, CreateCategoryDto dto)
        {
            var type = dto.Type.ToUpperInvariant();
            if (type is not ("EXPENSE" or "INCOME"))
                throw new InvalidOperationException("Type must be EXPENSE or INCOME.");

            var exists = await _db.Categories.AnyAsync(c =>
                c.UserId == userId && c.Name.ToLower() == dto.Name.ToLower() && c.Type == type);
            if (exists) throw new InvalidOperationException("Category with this name already exists.");

            var cat = new CategoryEntity
            {
                UserId = userId,
                Name = dto.Name.Trim(),
                Icon = dto.Icon,
                Color = dto.Color,
                Type = type,
                IsDefault = false,
                IsActive = true
            };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return ToResponse(cat);
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id)
                ?? throw new KeyNotFoundException($"Category {id} not found.");
            return ToResponse(c);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetDefaultsAsync() =>
            (await _db.Categories.AsNoTracking()
                .Where(c => c.IsDefault && c.IsActive)
                .OrderBy(c => c.Name).ToListAsync()).Select(ToResponse);

        public async Task<IEnumerable<CategoryResponseDto>> GetAllForUserAsync(int userId) =>
            (await _db.Categories.AsNoTracking()
                .Where(c => (c.UserId == null || c.UserId == userId) && c.IsActive)
                .OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync()).Select(ToResponse);

        public async Task<IEnumerable<CategoryResponseDto>> GetByTypeAsync(int userId, string type)
        {
            var t = type.ToUpperInvariant();
            return (await _db.Categories.AsNoTracking()
                .Where(c => (c.UserId == null || c.UserId == userId) && c.IsActive && c.Type == t)
                .OrderBy(c => c.Name).ToListAsync()).Select(ToResponse);
        }

        public async Task<CategoryResponseDto> UpdateAsync(int id, int userId, UpdateCategoryDto dto)
        {
            var c = await _db.Categories.FindAsync(id)
                ?? throw new KeyNotFoundException($"Category {id} not found.");
            if (c.IsDefault || c.UserId != userId)
                throw new UnauthorizedAccessException("You cannot update this category.");

            if (dto.Name != null)  c.Name  = dto.Name.Trim();
            if (dto.Icon != null)  c.Icon  = dto.Icon;
            if (dto.Color != null) c.Color = dto.Color;
            if (dto.Type != null)  c.Type  = dto.Type.ToUpperInvariant();

            await _db.SaveChangesAsync();
            return ToResponse(c);
        }

        public async Task DeactivateAsync(int id, int userId)
        {
            var rows = await _db.Categories
                .Where(c => c.CategoryId == id && c.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, false));
            if (rows == 0) throw new KeyNotFoundException($"Category {id} not found or not owned by user.");
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var rows = await _db.Categories
                .Where(c => c.CategoryId == id && c.UserId == userId && !c.IsDefault)
                .ExecuteDeleteAsync();
            if (rows == 0) throw new KeyNotFoundException($"Category {id} not found or not deletable.");
        }

        private static CategoryResponseDto ToResponse(CategoryEntity c) => new(
            c.CategoryId, 
            c.UserId, 
            c.Name, 
            c.Icon, 
            c.Color,
            c.Type, 
            c.IsDefault, 
            c.IsActive, 
            c.CreatedAt);
    }
}