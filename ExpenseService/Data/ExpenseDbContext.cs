using ExpenseService.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Data
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

        public DbSet<ExpenseEntity> Expenses => Set<ExpenseEntity>();
    }
}