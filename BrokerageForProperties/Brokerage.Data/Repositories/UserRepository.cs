using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;
using Dapper;

namespace Brokerage.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;


        public UserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        // Get user using email

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = """
            SELECT COUNT(1)
            FROM Users
            WHERE Email = @Email;
            """;
            using var connection =
              _connectionFactory.CreateConnection();
            int count = await connection.ExecuteScalarAsync<int>(
                sql,
                new { Email = email });

            return count > 0;
        }
        // Get role using role name

        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            const string query = """
                SELECT RoleID
                FROM Roles
                WHERE RoleName = @RoleName
                """;

            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<int?>(
                query,
                new { RoleName = roleName });
        }
        // Create new user

        public async Task<int> CreateUserAsync(User user)
        {
            const string query = """
            INSERT INTO Users
            (
                RoleID,
                FullName,
                Email,
                PasswordHash,
                MobileNumber
            )
            OUTPUT INSERTED.UserID
            VALUES
            (
                @RoleID,
                @FullName,
                @Email,
                @PasswordHash,
                @MobileNumber
            );
            """;

            using var connection =
          _connectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(
                query,
                user);
        }
        // Fetches all the User Details 
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string sql = """
    SELECT
        u.UserID,
        u.UserGUID,
        u.RoleID,
        r.RoleName,
        u.FullName,
        u.Email,
        u.PasswordHash,
        u.MobileNumber,
        u.IsActive,
        u.CreatedAt
    FROM Users u
    INNER JOIN Roles r
        ON u.RoleID = r.RoleID
    WHERE u.Email = @Email;
    """;
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User?>(sql, new { Email = email });

        }
        // Store [passwordreset token and expiration time in the db
        public async Task CreatePasswordResetTokenAsync(
    PasswordResetToken resetToken)
        {
            const string query = """
        INSERT INTO PasswordResetTokens
        (
            UserID,
            TokenHash,
            ExpiresAt
        )
        VALUES
        (
            @UserID,
            @TokenHash,
            @ExpiresAt
        );
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                query,
                resetToken);
        }
        // Get the password reset token using the token hash
        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(
    string tokenHash)
        {
            const string query = """
        SELECT
            ResetTokenID,
            UserID,
            TokenHash,
            ExpiresAt,
            UsedAt,
            CreatedAt
        FROM PasswordResetTokens
        WHERE TokenHash = @TokenHash;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<PasswordResetToken?>(
                query,
                new
                {
                    TokenHash = tokenHash
                });
        }
        public async Task MarkPasswordResetTokenUsedAsync(
    int resetTokenId)
        {
            const string query = """
        UPDATE PasswordResetTokens
        SET UsedAt = GETUTCDATE()
        WHERE ResetTokenID = @ResetTokenID;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                query,
                new
                {
                    ResetTokenID = resetTokenId
                });
        }
        public async Task UpdatePasswordAsync(
    int userId,
    string passwordHash)
        {
            const string query = """
        UPDATE Users
        SET PasswordHash = @PasswordHash
        WHERE UserID = @UserID;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                query,
                new
                {
                    UserID = userId,
                    PasswordHash = passwordHash
                });
        }
        public async Task InvalidateExistingPasswordResetTokensAsync(int userId)
        {
            const string query = """
        UPDATE PasswordResetTokens
        SET UsedAt = GETUTCDATE()
        WHERE UserID = @UserID
          AND UsedAt IS NULL;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                query,
                new
                {
                    UserID = userId
                });
        }
        public async Task<AdminUserListResponse>
     GetUsersForAdminAsync(
         AdminUserQueryRequest request)
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

            string? search =
                request.Search?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filters.Add(
                    "u.FullName LIKE @Search");

                parameters.Add(
                    "Search",
                    search + "%");
            }

            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                filters.Add(
                    "r.RoleName = @RoleName");

                parameters.Add(
                    "RoleName",
                    request.RoleName);
            }

            if (request.IsActive.HasValue)
            {
                filters.Add(
                    "u.IsActive = @IsActive");

                parameters.Add(
                    "IsActive",
                    request.IsActive.Value);
            }

            if (request.FromDate.HasValue)
            {
                filters.Add(
                    "u.CreatedAt >= @FromDate");

                parameters.Add(
                    "FromDate",
                    request.FromDate.Value.Date);
            }

            if (request.ToDate.HasValue)
            {
                filters.Add(
                    "u.CreatedAt < DATEADD(day, 1, @ToDate)");

                parameters.Add(
                    "ToDate",
                    request.ToDate.Value.Date);
            }

            string whereClause =
                filters.Count > 0
                    ? "WHERE " + string.Join(
                        " AND ",
                        filters)
                    : string.Empty;

            string countSql = $"""
        SELECT COUNT(1)
        FROM Users u
        INNER JOIN Roles r
            ON u.RoleID = r.RoleID
        {whereClause};
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            int totalRecords =
                await connection.ExecuteScalarAsync<int>(
                    countSql,
                    parameters);

            string dataSql = $"""
        SELECT
            u.UserID,
            u.UserGUID,
            u.FullName,
            u.Email,
            u.MobileNumber,
            r.RoleName,
            u.IsActive,
            u.CreatedAt,
            COALESCE(pc.ListingCount, 0) AS ListingCount
        FROM Users u
        INNER JOIN Roles r
            ON u.RoleID = r.RoleID
        LEFT JOIN
        (
            SELECT
                SellerID,
                COUNT(1) AS ListingCount
            FROM Properties
            GROUP BY SellerID
        ) pc
            ON pc.SellerID = u.UserID
        {whereClause}
        ORDER BY
            u.CreatedAt DESC,
            u.UserID DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

            parameters.Add(
                "Offset",
                offset);

            parameters.Add(
                "PageSize",
                pageSize);

            IEnumerable<AdminUserResponse> users =
                await connection.QueryAsync<AdminUserResponse>(
                    dataSql,
                    parameters);

            List<AdminUserResponse> userList =
                users.ToList();

            int totalPages =
                totalRecords == 0
                    ? 0
                    : (int)Math.Ceiling(
                        totalRecords /
                        (double)pageSize);

            return new AdminUserListResponse
            {
                Users = userList,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
        public async Task<int> GetTotalUserCountAsync()
        {
            const string sql = """
    SELECT COUNT(1)
    FROM Users;
    """;

            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }


}

