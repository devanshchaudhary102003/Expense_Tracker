using IncomeService.Models;
using Microsoft.EntityFrameworkCore;

namespace IncomeService.Data
{
    public class IncomeDbContext : DbContext
    {
        public IncomeDbContext(DbContextOptions<IncomeDbContext> options) : base(options) { }
        public DbSet<IncomeEntity> Incomes => Set<IncomeEntity>();
    }
}