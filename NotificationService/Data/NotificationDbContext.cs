using MassTransit.Contracts;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
        public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<NotificationEntity>(b =>
            {
                b.HasIndex(n => new { n.UserId, n.IsRead });
                b.HasIndex(n => new { n.UserId, n.SentAt });
                b.Property(n => n.SentAt).HasDefaultValueSql("NOW()");
            });
        }
    }
}