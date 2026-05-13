using NotificationService.DTOs;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> SendAsync(CreateNotificationDto dto);
        Task<NotificationResponseDto> SendBudgetAlertAsync(int userId, int budgetId, string budgetName, string alertType, decimal limitAmount, decimal spentAmount, decimal utilizationPercent);
        Task<IEnumerable<NotificationResponseDto>> SendBulkAsync(BroadcastNotificationDto dto, IList<int> resolvedUserIds);
            Task<IEnumerable<NotificationResponseDto>> GetByUserAsync(int userId, int limit = 50);
        Task<IEnumerable<NotificationResponseDto>> GetUnreadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllReadAsync(int userId);
        Task DeleteAsync(int notificationId, int userId);

    }
}