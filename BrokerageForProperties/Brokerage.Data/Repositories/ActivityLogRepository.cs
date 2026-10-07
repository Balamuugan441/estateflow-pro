using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;
using Dapper;

namespace Brokerage.Data.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ActivityLogRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(ActivityLog log)
    {
        const string sql = """
        INSERT INTO ActivityLogs
        (
            ActorUserID,
            ActorName,
            ActorRole,
            ActionType,
            ActionCategory,
            EntityType,
            EntityID,
            EntityName,
            Result,
            Details
        )
        VALUES
        (
            @ActorUserID,
            @ActorName,
            @ActorRole,
            @ActionType,
            @ActionCategory,
            @EntityType,
            @EntityID,
            @EntityName,
            @Result,
            @Details
        );
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, log);
    }

    public async Task<AdminActivityLogListResponse>
    GetActivityLogsForAdminAsync(
        AdminActivityLogQueryRequest request)
    {
        const int pageSize = 10;

        int pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        int offset =
            (pageNumber - 1) * pageSize;

        List<string> filters = [];

        DynamicParameters parameters =
            new DynamicParameters();

        // Date range filter
        if (request.FromDate.HasValue)
        {
            filters.Add(
                "CreatedAt >= @FromDate");

            parameters.Add(
                "FromDate",
                request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            filters.Add(
                "CreatedAt < DATEADD(day, 1, @ToDate)");

            parameters.Add(
                "ToDate",
                request.ToDate.Value.Date);
        }

        // Actor name search
        if (!string.IsNullOrWhiteSpace(
            request.ActorSearch))
        {
            filters.Add(
                "ActorName LIKE @ActorSearch");

            parameters.Add(
                "ActorSearch",
                $"%{request.ActorSearch.Trim()}%");
        }

        // Action category filter
        if (!string.IsNullOrWhiteSpace(
            request.ActionCategory))
        {
            filters.Add(
                "ActionCategory = @ActionCategory");

            parameters.Add(
                "ActionCategory",
                request.ActionCategory);
        }

        string whereClause =
            filters.Count > 0
                ? "WHERE " + string.Join(
                    " AND ",
                    filters)
                : string.Empty;

        string countSql = $"""
    SELECT COUNT(1)
    FROM ActivityLogs
    {whereClause};
    """;

        string dataSql = $"""
    SELECT
        ActivityLogID,
        ActorName,
        ActorRole,
        ActionType,
        ActionCategory,
        EntityType,
        EntityID,
        EntityName,
        Result,
        Details,
        CreatedAt
    FROM ActivityLogs
    {whereClause}
    ORDER BY
        CreatedAt DESC,
        ActivityLogID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    """;

        using var connection =
            _connectionFactory.CreateConnection();

        int totalRecords =
            await connection.ExecuteScalarAsync<int>(
                countSql,
                parameters);

        parameters.Add(
            "Offset",
            offset);

        parameters.Add(
            "PageSize",
            pageSize);

        IEnumerable<AdminActivityLogResponse> logs =
            await connection.QueryAsync<AdminActivityLogResponse>(
                dataSql,
                parameters);

        List<AdminActivityLogResponse> logList =
            logs.ToList();

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        return new AdminActivityLogListResponse
        {
            Logs = logList,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

}