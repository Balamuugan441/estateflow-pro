namespace Brokerage.Models.DTOs.Admin;

// Represents the result returned after an administrator changes a user's active status.
public class AdminUserStatusResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int UserID { get; set; }

    public bool IsActive { get; set; }
}