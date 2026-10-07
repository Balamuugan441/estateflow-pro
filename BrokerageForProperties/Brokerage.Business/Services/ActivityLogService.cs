using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Brokerage.Business.Services;
// Uses ActivityLogRepository To store the User/Properties/Admin logs in the Activity Logs Database
// Fetches the Saved Logs in the Database And Displays Them in the Admin Page For Monitory Purpose
public class ActivityLogService
{
    private readonly IActivityLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogService(
        IActivityLogRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        int? actorUserID,
        string? actorName,
        string? actorRole,
        string actionType,
        string actionCategory,
        string entityType,
        long? entityID,
        string? entityName,
        string result,
        string? details = null)
    {
        ActivityLog log = new ActivityLog
        {
            ActorUserID = actorUserID,
            ActorName = actorName,
            ActorRole = actorRole,
            ActionType = actionType,
            ActionCategory = actionCategory,
            EntityType = entityType,
            EntityID = entityID,
            EntityName = entityName,
            Result = result,
            Details = details
        };

        await _repository.CreateAsync(log);
    }

    public async Task LogCurrentUserAsync(
        string actionType,
        string actionCategory,
        string entityType,
        long? entityID,
        string? entityName,
        string result,
        string? details = null)
    {
        ClaimsPrincipal? user =
            _httpContextAccessor.HttpContext?.User;

        string? userIdValue =
            user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? actorUserID =
            int.TryParse(
                userIdValue,
                out int userId)
                ? userId
                : null;

        string? actorName =
            user?.FindFirst(ClaimTypes.Name)?.Value;

        string? actorRole =
            user?.FindFirst(ClaimTypes.Role)?.Value;

        if (actorUserID == null)
        {
            actorName ??= "System";
            actorRole ??= "System";
        }

        await LogAsync(
            actorUserID,
            actorName,
            actorRole,
            actionType,
            actionCategory,
            entityType,
            entityID,
            entityName,
            result,
            details);
    }
    public async Task<AdminActivityLogListResponse>
    GetActivityLogsForAdminAsync(
        AdminActivityLogQueryRequest request)
    {
        return await _repository
            .GetActivityLogsForAdminAsync(request);
    }


}