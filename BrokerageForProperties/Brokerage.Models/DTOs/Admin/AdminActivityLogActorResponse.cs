namespace Brokerage.Models.DTOs.Admin;

public class AdminActivityLogActorResponse
{
    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? RoleName { get; set; }
}