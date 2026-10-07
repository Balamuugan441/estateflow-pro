using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Notifications;

namespace Brokerage.Business.Services;


// Handles retrieval and read-state management for user notifications.

public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        INotificationRepository notificationRepository)
    {
        _notificationRepository =
            notificationRepository;
    }

    public async Task<List<NotificationDto>> GetUnreadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository
            .GetUnreadAsync(
                userId,
                cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository
            .MarkAsReadAsync(
                userId,
                notificationId,
                cancellationToken);
    }
    public async Task<List<NotificationDto>> GetAllAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        return await _notificationRepository
            .GetAllAsync(
                userId,
                cancellationToken);
    }
}