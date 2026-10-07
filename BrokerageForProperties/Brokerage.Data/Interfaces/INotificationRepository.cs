using Brokerage.Models.DTOs.Notifications;

namespace Brokerage.Data.Interfaces;


// Provides database operations for user notifications.

public interface INotificationRepository
{
    Task<List<NotificationDto>> GetUnreadAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetAllAsync(
    int userId,
    CancellationToken cancellationToken = default);
}