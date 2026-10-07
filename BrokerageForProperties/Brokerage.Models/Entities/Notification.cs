namespace Brokerage.Models.Entities;


// Represents a persistent notification delivered to an application user.

public class Notification
{
    public int NotificationID { get; set; }

    public int RecipientUserID { get; set; }

    public int? SenderUserID { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityID { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}