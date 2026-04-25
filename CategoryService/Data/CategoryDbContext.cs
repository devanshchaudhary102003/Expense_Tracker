using CategoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Data
{
    public class CategoryDbContext : DbContext
    {
        public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options) { }
        public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryEntity>(b =>
            {
                b.HasIndex(c => new { c.UserId, c.Name });
                b.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            });

            // System-default categories (UserId = null, IsDefault = true, visible to all users)
            var seed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<CategoryEntity>().HasData(
                new CategoryEntity { CategoryId = 1,  Name = "Food",          Icon = "🍔", Color = "#ef4444", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 2,  Name = "Transport",     Icon = "🚗", Color = "#3b82f6", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 3,  Name = "Entertainment", Icon = "🎬", Color = "#a855f7", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 4,  Name = "Health",        Icon = "💊", Color = "#10b981", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 5,  Name = "Shopping",      Icon = "🛍", Color = "#f59e0b", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 6,  Name = "Bills",         Icon = "💡", Color = "#6366f1", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 7,  Name = "Education",     Icon = "📚", Color = "#14b8a6", Type = "EXPENSE", IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 8,  Name = "Salary",        Icon = "💰", Color = "#22c55e", Type = "INCOME",  IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 9,  Name = "Freelance",     Icon = "💻", Color = "#06b6d4", Type = "INCOME",  IsDefault = true, IsActive = true, CreatedAt = seed },
                new CategoryEntity { CategoryId = 10, Name = "Investment",    Icon = "📈", Color = "#84cc16", Type = "INCOME",  IsDefault = true, IsActive = true, CreatedAt = seed }
            );
        }
    }
}