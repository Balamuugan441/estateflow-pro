using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.Entities;

namespace Brokerage.Data.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<int?> GetRoleIdByNameAsync(string roleName);

    Task<int> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task CreatePasswordResetTokenAsync(
    PasswordResetToken resetToken);

    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash);

    Task MarkPasswordResetTokenUsedAsync(int resetTokenId);

    Task UpdatePasswordAsync(int userId, string passwordHash);
    Task InvalidateExistingPasswordResetTokensAsync(int userId);
    Task<AdminUserListResponse> GetUsersForAdminAsync(
        AdminUserQueryRequest request);
    Task<int> GetTotalUserCountAsync();
}