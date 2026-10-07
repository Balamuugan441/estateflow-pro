using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

public class AdminDashboardApiResponse
{
    public AdminDashboardSummaryApiResponse Summary { get; set; } = new();

    public List<AdminDashboardCountApiModel> PropertyStatuses { get; set; } = [];

    public List<AdminDashboardTrendApiModel> UserDailyTrend { get; set; } = [];

    public List<AdminDashboardTrendApiModel> UserMonthlyTrend { get; set; } = [];

    public List<AdminDashboardTrendApiModel> PropertyDailyTrend { get; set; } = [];

    public List<AdminDashboardTrendApiModel> PropertyMonthlyTrend { get; set; } = [];

    public List<AdminDashboardRecentUserApiModel> RecentUsers { get; set; } = [];

    public List<AdminDashboardPendingPropertyApiModel> PendingProperties { get; set; } = [];
}