namespace Brokerage.Models.DTOs.Admin;

public class AdminActivityLogResponse
{
    public long ActivityLogID { get; set; }

    public string ActorName { get; set; } = "System";
    public string? ActorRole { get; set; }

    public string ActionType { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;
    public long? EntityID { get; set; }
    public string? EntityName { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }
}