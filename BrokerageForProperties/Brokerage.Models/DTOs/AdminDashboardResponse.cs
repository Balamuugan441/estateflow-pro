namespace Brokerage.Models.DTOs.Admin;

public class AdminDashboardResponse
{
    public AdminDashboardSummary Summary { get; set; } = new();

    public List<AdminDashboardCount> PropertyStatuses { get; set; } = [];

    public List<AdminDashboardTrendPoint> UserDailyTrend { get; set; } = [];

    public List<AdminDashboardTrendPoint> UserMonthlyTrend { get; set; } = [];

    public List<AdminDashboardTrendPoint> PropertyDailyTrend { get; set; } = [];

    public List<AdminDashboardTrendPoint> PropertyMonthlyTrend { get; set; } = [];

    public List<AdminDashboardRecentUser> RecentUsers { get; set; } = [];

    public List<AdminDashboardPendingProperty> PendingProperties { get; set; } = [];
}