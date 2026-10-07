using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;

namespace Brokerage.Data.Interfaces;

public interface IActivityLogRepository
{
    Task CreateAsync(ActivityLog log);

    Task<AdminActivityLogListResponse>
        GetActivityLogsForAdminAsync(
            AdminActivityLogQueryRequest request);
}