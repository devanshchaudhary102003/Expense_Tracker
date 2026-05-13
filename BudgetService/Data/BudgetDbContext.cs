using BudgetService.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.Data
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options){ }
        public DbSet<BudgetEntity> Budgets => Set<BudgetEntity>();
    }
}