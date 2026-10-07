namespace Brokerage.Web.Models.ApiModels;

// Represents a notification returned by the Brokerage API.

public class NotificationApiModel
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