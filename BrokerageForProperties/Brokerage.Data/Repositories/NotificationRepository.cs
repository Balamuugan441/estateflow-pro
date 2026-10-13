using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Notifications;
using Dapper;
using System.Data;

namespace Brokerage.Data.Repositories;

// Handles database operations for user notifications.

public class NotificationRepository : INotificationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public NotificationRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory;
    }

    public async Task<List<NotificationDto>> GetUnreadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string query = """
    SELECT
        NotificationID,
        NotificationType,
        Title,
        Message,
        RelatedEntityType,
        RelatedEntityID,
        IsRead,
        CreatedAt
    FROM dbo.Notifications
    WHERE RecipientUserID = @UserID
      AND IsRead = 0
    ORDER BY
        CreatedAt DESC,
        NotificationID DESC;
    """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<NotificationDto> notifications =
            await connection.QueryAsync<NotificationDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        UserID = userId
                    },
                    cancellationToken:
                        cancellationToken));

        return notifications.ToList();
    }

    public async Task<bool> MarkAsReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            UPDATE dbo.Notifications
            SET IsRead = 1
            WHERE NotificationID = @NotificationID
              AND RecipientUserID = @UserID
              AND IsRead = 0;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                new CommandDefinition(
                    query,
                    new
                    {
                        NotificationID =
                            notificationId,

                        UserID =
                            userId
                    },
                    cancellationToken:
                        cancellationToken));

        return rowsAffected > 0;
    }
    public async Task<List<NotificationDto>> GetAllAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        const string query = """
        SELECT
            NotificationID,
            RecipientUserID,
            SenderUserID,
            NotificationType,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityID,
            IsRead,
            CreatedAt
        FROM dbo.Notifications
        WHERE RecipientUserID = @UserID
        ORDER BY
            CreatedAt DESC,
            NotificationID DESC;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<NotificationDto> notifications =
            await connection.QueryAsync<NotificationDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        UserID = userId
                    },
                    cancellationToken:
                        cancellationToken));

        return notifications.ToList();
    }
    public async Task<bool> CreateAsync(
    int recipientUserId,
    string notificationType,
    string title,
    string message,
    string? relatedEntityType,
    int? relatedEntityId,
    CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "dbo.usp_Notifications_Create",
                    new
                    {
                        RecipientUserID = recipientUserId,
                        NotificationType = notificationType,
                        Title = title,
                        Message = message,
                        RelatedEntityType = relatedEntityType,
                        RelatedEntityID = relatedEntityId
                    },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

        return rowsAffected > 0;
    }
}