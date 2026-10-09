using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;
using Brokerage.Models.Enums;
using Dapper;
using System.Data;

namespace Brokerage.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Check whether email already exists
        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection =
                _connectionFactory.CreateConnection();

            int count =
                await connection.ExecuteScalarAsync<int>(
                    "dbo.usp_User_EmailExists",
                    new
                    {
                        Email = email
                    },
                    commandType: CommandType.StoredProcedure);

            return count > 0;
        }

        // Get role ID using role name
        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<int?>(
                "dbo.usp_Role_GetIdByName",
                new
                {
                    RoleName = roleName
                },
                commandType: CommandType.StoredProcedure);
        }

        // Create new user
        public async Task<int> CreateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(
                "dbo.usp_User_Create",
                user,
                commandType: CommandType.StoredProcedure);
        }

        // Get user using email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();


            return await connection.QuerySingleOrDefaultAsync<User?>(
                "dbo.usp_User_GetByEmail",
                new
                {
                    Email = email
                },
                commandType: CommandType.StoredProcedure);
        }

        // Store password reset token
        public async Task CreatePasswordResetTokenAsync(
            PasswordResetToken resetToken)
        {
            using var connection = _connectionFactory.CreateConnection();


            await connection.ExecuteAsync(
                "dbo.usp_PasswordResetToken_Create",
                resetToken,
                commandType: CommandType.StoredProcedure);
        }

        // Get password reset token
        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(
            string tokenHash)
        {
            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<PasswordResetToken?>(
                "dbo.usp_PasswordResetToken_GetByHash",
                new
                {
                    TokenHash = tokenHash
                },
                commandType: CommandType.StoredProcedure);
        }

        // Mark password reset token as used
        public async Task MarkPasswordResetTokenUsedAsync(int resetTokenId)

        {
            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "dbo.usp_PasswordResetToken_MarkUsed",
                new
                {
                    ResetTokenID = resetTokenId
                },
                commandType: CommandType.StoredProcedure);
        }

        // Update password
        public async Task UpdatePasswordAsync(int userId, string passwordHash)

        {
            using var connection = _connectionFactory.CreateConnection();


            await connection.ExecuteAsync(
                "dbo.usp_User_UpdatePassword",
                new
                {
                    UserID = userId,
                    PasswordHash = passwordHash
                },
                commandType: CommandType.StoredProcedure);
        }

        // Invalidate existing password reset tokens
        public async Task InvalidateExistingPasswordResetTokensAsync(
            int userId)
        {
            using var connection = _connectionFactory.CreateConnection();


            await connection.ExecuteAsync("dbo.usp_PasswordResetToken_InvalidateExisting",

                new
                {
                    UserID = userId
                },
                commandType: CommandType.StoredProcedure);
        }

        // Get paginated users for admin
        public async Task<AdminUserListResponse> GetUsersForAdminAsync(
            AdminUserQueryRequest request)
        {
            const int pageSize = 10;

            int pageNumber =
                request.PageNumber < 1
                    ? 1
                    : request.PageNumber;

            string? search = request.Search?.Trim();


            using var connection = _connectionFactory.CreateConnection();


            var parameters = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,

                Search = string.IsNullOrWhiteSpace(search)
                    ? null
                    : search,

                RoleName = string.IsNullOrWhiteSpace(request.RoleName)
                    ? null
                    : request.RoleName,

                IsActive = request.IsActive,

                FromDate = request.FromDate?.Date,

                ToDate = request.ToDate?.Date
            };

            using var multi =
                await connection.QueryMultipleAsync(
                    "dbo.usp_Admin_User_GetPaged",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            int totalRecords = await multi.ReadSingleAsync<int>();


            IEnumerable<AdminUserResponse> users = await multi.ReadAsync<AdminUserResponse>();


            List<AdminUserResponse> userList = users.ToList();


            int totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize);


            return new AdminUserListResponse
            {
                Users = userList,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        // Get total user count
        public async Task<int> GetTotalUserCountAsync()
        {
            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(
                "dbo.usp_User_GetTotalCount",
                commandType: CommandType.StoredProcedure);
        }
        // Updates the user's IsActive status through the admin user-status stored procedure.
        public async Task<bool> UpdateUserStatusAsync(int userId, UserStatus status)
        {
            using var connection = _connectionFactory.CreateConnection();
            bool isActive = status == UserStatus.Active;

            int rowsAffected = await connection.ExecuteScalarAsync<int>(
           "dbo.usp_Admin_User_UpdateStatus",
           new
           {
               UserID = userId,
               IsActive = isActive
           },
           commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }
    }
}