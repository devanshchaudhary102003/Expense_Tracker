using Microsoft.EntityFrameworkCore;
using ReportService.Models;

namespace ReportService.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }
        public DbSet<ReportEntity> Reports => Set<ReportEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReportEntity>(b =>
            {
                b.HasIndex(r => new { r.UserId, r.GeneratedAt });
                b.Property(r => r.GeneratedAt).HasDefaultValueSql("NOW()");
            });
        }
    }
}