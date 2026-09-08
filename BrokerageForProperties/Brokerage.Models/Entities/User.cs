namespace Brokerage.Models.Entities;
// Complete Information about the User to be Stored in the User Table on the Database.
public class User
{
    public int UserID { get; set; }

    public Guid UserGUID { get; set; }

    public int RoleID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}