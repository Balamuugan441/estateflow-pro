namespace Brokerage.Web.Models.ApiResponses;

public class AdminActivityLogActorApiModel
{
    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? RoleName { get; set; }
}