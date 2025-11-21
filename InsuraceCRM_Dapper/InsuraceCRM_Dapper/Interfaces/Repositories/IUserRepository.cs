using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IUserRepository
{
    Task<int> InsertAsync(User user);
    Task UpdateAsync(User user);
    Task UpdateRoleAsync(int userId, string role);
    Task DeleteAsync(int id);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
}
