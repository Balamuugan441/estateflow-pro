namespace Brokerage.Models.DTOs.Admin;

public class AdminDashboardSummary
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int Buyers { get; set; }

    public int Sellers { get; set; }

    public int Admins { get; set; }

    public int NewUsersToday { get; set; }

    public int NewUsersLast7Days { get; set; }

    public int NewUsersLast30Days { get; set; }

    public int NewUsersLast365Days { get; set; }

    public int TotalProperties { get; set; }

    public int DraftProperties { get; set; }

    public int PendingProperties { get; set; }

    public int ApprovedProperties { get; set; }

    public int RejectedProperties { get; set; }

    public int NewPropertiesLast7Days { get; set; }

    public int NewPropertiesLast30Days { get; set; }

    public int NewPropertiesLast365Days { get; set; }
}