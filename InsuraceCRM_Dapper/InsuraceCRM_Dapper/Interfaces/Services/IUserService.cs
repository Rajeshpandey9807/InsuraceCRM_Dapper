using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<IEnumerable<User>> GetAllUsersAsync(bool includeInactive = false);
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateUserAsync(User user, string password);
    Task UpdateUserAsync(User user, string? newPassword = null);
    Task UpdateRoleAsync(int userId, int role);
    Task SetActiveStateAsync(int userId, bool isActive);
    Task EnsureDefaultAdminAsync();
    Task<IEnumerable<Role>> GetRolesAsync();
}
