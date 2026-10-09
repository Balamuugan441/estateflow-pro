namespace Brokerage.Web.Models.ApiResponses;

// Represents the API response used by the Admin Users page after a status update.
public class AdminUserStatusApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int UserID { get; set; }

    public bool IsActive { get; set; }
}