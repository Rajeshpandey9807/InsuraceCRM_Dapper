using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IUserRepository
{
    Task<int> InsertAsync(User user);
    Task UpdateAsync(User user);
    Task UpdateRoleAsync(int userId, int role);
    Task DeleteAsync(int id);
    Task SetActiveStateAsync(int userId, bool isActive);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Role>> GetRolesAsync();
    Task<int> InsertRoleAsync(Role role);
    Task UpdateRoleNameAsync(Role role);
    Task DeleteRoleAsync(int roleId);
    Task<bool> RoleHasUsersAsync(int roleId);
}
