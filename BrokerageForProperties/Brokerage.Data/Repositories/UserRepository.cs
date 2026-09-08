using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.Entities;
using Dapper;
using System.Data.Common;

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
                SELECT * FROM Users
                WHERE Email = @Email;
                """;
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User?>(sql, new { Email = email });

        }

    }
}