using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly NotificationDbContext _db;
        private readonly IEmailSender _email;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(NotificationDbContext db, IEmailSender email, ILogger<NotificationServiceImpl> logger)
        {
            _db = db;
            _email = email;
            _logger = logger;
        }

        public async Task<NotificationResponseDto> SendAsync(CreateNotificationDto dto)
        {
            var n = new NotificationEntity
            {
                UserId = dto.UserId,
                Type = dto.Type.ToUpperInvariant(),
                Title = dto.Title,
                Message = dto.Message,
                RelatedId = dto.RelatedId,
                IsRead = false  
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
            return ToResponse(n);
        }

        public async Task<NotificationResponseDto> SendBudgetAlertAsync(int userId, int budgetId, string budgetName, string alertType, decimal limitAmount, decimal spentAmount, decimal utilizationPercent)
        {
            var type = alertType == "LIMIT_REACHED" ? "BUDGET_EXCEEDED" : "BUDGET_WARNING";
            var title = alertType == "LIMIT_REACHED" ? $"Budget Exceeded: {budgetName}" : $"Budget Warning: {budgetName}";
            var message = alertType == "LIMIT_REACHED" ? $"You have exceeded your '{budgetName}' budget. Spent {spentAmount:N2} of {limitAmount:N2} ({utilizationPercent}%)." : $"You have used {utilizationPercent}% of your '{budgetName}' budget ({spentAmount:N2} of {limitAmount:N2}).";

            var n = new NotificationEntity
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                RelatedId = budgetId,
                IsRead = false  
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            // Fire-and-forget email for the critical LIMIT_REACHED alert.
            // In production: look up the user's email via AuthService; here we log instead.
            if(alertType == "LIMIT_REACHED")
            {
                _logger.LogInformation("Would email user {UserId} about budget {BudgetId} exceeded.", userId, budgetId);
                // await _email.SendAsync(userEmail, title, $"<p>{message}</p>");
            }

            return ToResponse(n);
        }

        public async Task<IEnumerable<NotificationResponseDto>> SendBulkAsync(BroadcastNotificationDto dto, IList<int> resolvedUserIds)
        {
            if (resolvedUserIds.Count == 0) return Array.Empty<NotificationResponseDto>();

            var now = DateTime.UtcNow;
            var entities = resolvedUserIds.Select(uid => new NotificationEntity
            {
                UserId = uid,
                Type = "PLATFORM",
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                SentAt = now
            }).ToList();

            _db.Notifications.AddRange(entities);
            await _db.SaveChangesAsync();
            return entities.Select(ToResponse);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetByUserAsync(int userId, int limit = 50) =>
            (await _db.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Take(limit)
                .ToListAsync()).Select(ToResponse);

        public async Task<IEnumerable<NotificationResponseDto>> GetUnreadAsync(int userId) =>
            (await _db.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync()).Select(ToResponse);

        public async Task<int> GetUnreadCountAsync(int userId) =>
        await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var rows = await _db.Notifications
                .Where(n => n.NotificationId == notificationId && n.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            if (rows == 0) throw new KeyNotFoundException($"Notification {notificationId} not found.");
        }

        public async Task MarkAllReadAsync(int userId)
        {
            await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task DeleteAsync(int notificationId, int userId)
        {
            var rows = await _db.Notifications
                .Where(n => n.NotificationId == notificationId && n.UserId == userId)
                .ExecuteDeleteAsync();
            if (rows == 0) throw new KeyNotFoundException($"Notification {notificationId} not found.");
        }
        private static NotificationResponseDto ToResponse(NotificationEntity n) => new(
            n.NotificationId,
            n.UserId, 
            n.Type, 
            n.Title, 
            n.Message,
            n.RelatedId, 
            n.IsRead, 
            n.SentAt);
    }
}