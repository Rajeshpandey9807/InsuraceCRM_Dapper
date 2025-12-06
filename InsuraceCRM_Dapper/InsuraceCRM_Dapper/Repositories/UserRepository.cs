using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (FullName, Email, PasswordHash, Mobile, RoleId, IsActive)
            VALUES (@Name, @Email, @PasswordHash, @Mobile, @RoleId, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE Users
            SET FullName = @Name,
                Email = @Email,
                PasswordHash = @PasswordHash,
                Mobile = @Mobile,
                RoleId = @RoleId,
                IsActive = @IsActive
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateRoleAsync(int userId, string role)
    {
        const string sql = "UPDATE Users SET RoleId = @RoleId WHERE Id = @UserId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Role = role, UserId = userId });
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task SetActiveStateAsync(int userId, bool isActive)
    {
        const string sql = "UPDATE Users SET IsActive = @IsActive WHERE Id = @UserId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { UserId = userId, IsActive = isActive });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
    {
        const string sql = @"
            SELECT *
            FROM Users
            WHERE IsActive = 1
            ORDER BY FullName;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<User>(sql, new { IncludeInactive = includeInactive });
    }
}
