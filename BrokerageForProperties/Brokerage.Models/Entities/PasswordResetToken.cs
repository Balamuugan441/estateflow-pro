namespace Brokerage.Models.Entities;

public class PasswordResetToken
{
    public int ResetTokenID { get; set; }

    public int UserID { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}