using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task UpdateRoleAsync(int userId, string role);
    Task<User?> GetByIdAsync(int id);
    Task EnsureDefaultAdminAsync();
}
