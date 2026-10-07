using Brokerage.Models.DTOs.Admin;

namespace Brokerage.Data.Interfaces;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardResponse>
        GetDashboardAsync();
}