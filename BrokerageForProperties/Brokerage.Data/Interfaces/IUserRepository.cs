using Brokerage.Models.Entities;

namespace Brokerage.Data.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<int?> GetRoleIdByNameAsync(string roleName);

    Task<int> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
}