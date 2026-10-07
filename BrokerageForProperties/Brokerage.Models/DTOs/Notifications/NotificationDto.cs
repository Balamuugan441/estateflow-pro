namespace Brokerage.Models.DTOs.Notifications;

// Represents a notification returned to the authenticated application user.

public class NotificationDto
{
    public int NotificationID { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityID { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}