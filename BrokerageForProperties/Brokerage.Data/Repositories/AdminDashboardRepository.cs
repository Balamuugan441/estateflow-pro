using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Dapper;

namespace Brokerage.Data.Repositories;

public class AdminDashboardRepository :
    IAdminDashboardRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AdminDashboardRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AdminDashboardResponse>
        GetDashboardAsync()
    {
        const string query = """
            SELECT
                COUNT(1) AS TotalUsers,

                COUNT(
                    CASE
                        WHEN u.IsActive = 1
                        THEN 1
                    END) AS ActiveUsers,

                COUNT(
                    CASE
                        WHEN r.RoleName = 'Buyer'
                        THEN 1
                    END) AS Buyers,

                COUNT(
                    CASE
                        WHEN r.RoleName = 'Seller'
                        THEN 1
                    END) AS Sellers,

                COUNT(
                    CASE
                        WHEN r.RoleName = 'Admin'
                        THEN 1
                    END) AS Admins,

                COUNT(
                    CASE
                        WHEN u.CreatedAt >= CAST(GETDATE() AS date)
                        THEN 1
                    END) AS NewUsersToday,

                COUNT(
                    CASE
                        WHEN u.CreatedAt >=
                            DATEADD(
                                day,
                                -6,
                                CAST(GETDATE() AS date))
                        THEN 1
                    END) AS NewUsersLast7Days,

                COUNT(
                    CASE
                        WHEN u.CreatedAt >=
                            DATEADD(
                                day,
                                -29,
                                CAST(GETDATE() AS date))
                        THEN 1
                    END) AS NewUsersLast30Days,

                COUNT(
                    CASE
                        WHEN u.CreatedAt >=
                            DATEADD(
                                day,
                                -364,
                                CAST(GETDATE() AS date))
                        THEN 1
                    END) AS NewUsersLast365Days,

                (
                    SELECT COUNT(1)
                    FROM Properties
                ) AS TotalProperties,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE ListingStatus = 'Draft'
                ) AS DraftProperties,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE ListingStatus = 'Pending'
                ) AS PendingProperties,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE ListingStatus = 'Approved'
                ) AS ApprovedProperties,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE ListingStatus = 'Rejected'
                ) AS RejectedProperties,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE CreatedAt >=
                        DATEADD(
                            day,
                            -6,
                            CAST(GETDATE() AS date))
                ) AS NewPropertiesLast7Days,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE CreatedAt >=
                        DATEADD(
                            day,
                            -29,
                            CAST(GETDATE() AS date))
                ) AS NewPropertiesLast30Days,

                (
                    SELECT COUNT(1)
                    FROM Properties
                    WHERE CreatedAt >=
                        DATEADD(
                            day,
                            -364,
                            CAST(GETDATE() AS date))
                ) AS NewPropertiesLast365Days

            FROM Users u
            INNER JOIN Roles r
                ON u.RoleID = r.RoleID;


            WITH Statuses AS
            (
                SELECT StatusName
                FROM
                (
                    VALUES
                        ('Draft'),
                        ('Pending'),
                        ('Approved'),
                        ('Rejected')
                ) AS StatusList(StatusName)
            )
            SELECT
                s.StatusName AS Name,
                COUNT(p.PropertyID) AS Count
            FROM Statuses s
            LEFT JOIN Properties p
                ON p.ListingStatus = s.StatusName
            GROUP BY
                s.StatusName
            ORDER BY
                CASE s.StatusName
                    WHEN 'Approved' THEN 1
                    WHEN 'Pending' THEN 2
                    WHEN 'Draft' THEN 3
                    WHEN 'Rejected' THEN 4
                    ELSE 5
                END;


            WITH Dates AS
            (
                SELECT
                    CAST(
                        DATEADD(
                            day,
                            -29,
                            CAST(GETDATE() AS date)
                        ) AS date
                    ) AS PeriodStart

                UNION ALL

                SELECT
                    DATEADD(
                        day,
                        1,
                        PeriodStart
                    )
                FROM Dates
                WHERE PeriodStart <
                    CAST(GETDATE() AS date)
            )
            SELECT
                d.PeriodStart,
                COUNT(u.UserID) AS Count
            FROM Dates d
            LEFT JOIN Users u
                ON u.CreatedAt >= d.PeriodStart
                AND u.CreatedAt <
                    DATEADD(
                        day,
                        1,
                        d.PeriodStart
                    )
            GROUP BY
                d.PeriodStart
            ORDER BY
                d.PeriodStart
            OPTION (MAXRECURSION 100);


            WITH Months AS
            (
                SELECT
                    DATEFROMPARTS(
                        YEAR(
                            DATEADD(
                                month,
                                -11,
                                GETDATE()
                            )
                        ),
                        MONTH(
                            DATEADD(
                                month,
                                -11,
                                GETDATE()
                            )
                        ),
                        1
                    ) AS PeriodStart

                UNION ALL

                SELECT
                    DATEADD(
                        month,
                        1,
                        PeriodStart
                    )
                FROM Months
                WHERE PeriodStart <
                    DATEFROMPARTS(
                        YEAR(GETDATE()),
                        MONTH(GETDATE()),
                        1
                    )
            )
            SELECT
                m.PeriodStart,
                COUNT(u.UserID) AS Count
            FROM Months m
            LEFT JOIN Users u
                ON u.CreatedAt >= m.PeriodStart
                AND u.CreatedAt <
                    DATEADD(
                        month,
                        1,
                        m.PeriodStart
                    )
            GROUP BY
                m.PeriodStart
            ORDER BY
                m.PeriodStart
            OPTION (MAXRECURSION 20);


            WITH Dates AS
            (
                SELECT
                    CAST(
                        DATEADD(
                            day,
                            -29,
                            CAST(GETDATE() AS date)
                        ) AS date
                    ) AS PeriodStart

                UNION ALL

                SELECT
                    DATEADD(
                        day,
                        1,
                        PeriodStart
                    )
                FROM Dates
                WHERE PeriodStart <
                    CAST(GETDATE() AS date)
            )
            SELECT
                d.PeriodStart,
                COUNT(p.PropertyID) AS Count
            FROM Dates d
            LEFT JOIN Properties p
                ON p.CreatedAt >= d.PeriodStart
                AND p.CreatedAt <
                    DATEADD(
                        day,
                        1,
                        d.PeriodStart
                    )
            GROUP BY
                d.PeriodStart
            ORDER BY
                d.PeriodStart
            OPTION (MAXRECURSION 100);


            WITH Months AS
            (
                SELECT
                    DATEFROMPARTS(
                        YEAR(
                            DATEADD(
                                month,
                                -11,
                                GETDATE()
                            )
                        ),
                        MONTH(
                            DATEADD(
                                month,
                                -11,
                                GETDATE()
                            )
                        ),
                        1
                    ) AS PeriodStart

                UNION ALL

                SELECT
                    DATEADD(
                        month,
                        1,
                        PeriodStart
                    )
                FROM Months
                WHERE PeriodStart <
                    DATEFROMPARTS(
                        YEAR(GETDATE()),
                        MONTH(GETDATE()),
                        1
                    )
            )
            SELECT
                m.PeriodStart,
                COUNT(p.PropertyID) AS Count
            FROM Months m
            LEFT JOIN Properties p
                ON p.CreatedAt >= m.PeriodStart
                AND p.CreatedAt <
                    DATEADD(
                        month,
                        1,
                        m.PeriodStart
                    )
            GROUP BY
                m.PeriodStart
            ORDER BY
                m.PeriodStart
            OPTION (MAXRECURSION 20);


            SELECT TOP 5
                u.UserID,
                u.FullName,
                u.Email,
                r.RoleName,
                u.IsActive,
                u.CreatedAt
            FROM Users u
            INNER JOIN Roles r
                ON u.RoleID = r.RoleID
            ORDER BY
                u.CreatedAt DESC,
                u.UserID DESC;


            SELECT TOP 5
                p.PropertyID,
                p.PropertyTitle,
                u.FullName AS SellerName,
                p.City,
                p.Price,
                p.ListingStatus,
                COALESCE(
                    p.UpdatedAt,
                    p.CreatedAt
                ) AS SubmittedAt
            FROM Properties p
            INNER JOIN Users u
                ON p.SellerID = u.UserID
            WHERE p.ListingStatus = 'Pending'
            ORDER BY
                COALESCE(
                    p.UpdatedAt,
                    p.CreatedAt
                ) DESC,
                p.PropertyID DESC;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        using SqlMapper.GridReader results =
            await connection.QueryMultipleAsync(
                query);

        AdminDashboardSummary summary =
            await results.ReadSingleAsync<
                AdminDashboardSummary>();

        IEnumerable<AdminDashboardCount>
            statuses =
                await results.ReadAsync<
                    AdminDashboardCount>();

        IEnumerable<AdminDashboardTrendPoint>
            userDaily =
                await results.ReadAsync<
                    AdminDashboardTrendPoint>();

        IEnumerable<AdminDashboardTrendPoint>
            userMonthly =
                await results.ReadAsync<
                    AdminDashboardTrendPoint>();

        IEnumerable<AdminDashboardTrendPoint>
            propertyDaily =
                await results.ReadAsync<
                    AdminDashboardTrendPoint>();

        IEnumerable<AdminDashboardTrendPoint>
            propertyMonthly =
                await results.ReadAsync<
                    AdminDashboardTrendPoint>();

        IEnumerable<AdminDashboardRecentUser>
            recentUsers =
                await results.ReadAsync<
                    AdminDashboardRecentUser>();

        IEnumerable<AdminDashboardPendingProperty>
            pendingProperties =
                await results.ReadAsync<
                    AdminDashboardPendingProperty>();

        return new AdminDashboardResponse
        {
            Summary = summary,
            PropertyStatuses = statuses.ToList(),
            UserDailyTrend = userDaily.ToList(),
            UserMonthlyTrend = userMonthly.ToList(),
            PropertyDailyTrend = propertyDaily.ToList(),
            PropertyMonthlyTrend = propertyMonthly.ToList(),
            RecentUsers = recentUsers.ToList(),
            PendingProperties = pendingProperties.ToList()
        };
    }
}