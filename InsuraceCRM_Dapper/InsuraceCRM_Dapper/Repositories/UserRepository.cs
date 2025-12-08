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
        //const string sql = @"
        //    INSERT INTO Users (Name, Email, PasswordHash, Mobile, Role, IsActive)
        //    VALUES (@Name, @Email, @PasswordHash, @Mobile, @Role, @IsActive);
        //    SELECT CAST(SCOPE_IDENTITY() as int);";
        const string sql = @"INSERT INTO Users (FullName, Email, PasswordHash, Mobile, RoleId, IsActive)
            VALUES (@FullName, @Email, @PasswordHash, @Mobile, @RoleId, @IsActive)           
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        const string sql = "SELECT RoleId, RoleName FROM Roles ORDER BY RoleName;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Role>(sql);
    }

    public async Task<int> InsertRoleAsync(Role role)
    {
        const string sql = @"INSERT INTO Roles (RoleName)
                             VALUES (@RoleName);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { role.RoleName });
    }

    public async Task UpdateRoleNameAsync(Role role)
    {
        const string sql = "UPDATE Roles SET RoleName = @RoleName WHERE RoleId = @RoleId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { role.RoleName, role.RoleId });
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        const string sql = "DELETE FROM Roles WHERE RoleId = @RoleId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { RoleId = roleId });
    }

    public async Task<bool> RoleHasUsersAsync(int roleId)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE RoleId = @RoleId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { RoleId = roleId });
        return count > 0;
    }
    public async Task UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE Users
            SET FullName = @FullName,
                Email = @Email,
                PasswordHash = @PasswordHash,
                Mobile = @Mobile,
                RoleId=@RoleId,
                IsActive = @IsActive
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateRoleAsync(int userId, int RoleId)
    {
        const string sql = "UPDATE Users SET RoleId = @RoleId WHERE Id = @UserId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { RoleId = RoleId, UserId = userId });
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
        const string sql = @"
            SELECT u.Id,
                   u.FullName,
                   u.Email,
                   u.PasswordHash,
                   u.Mobile,
                   u.RoleId,
                   r.RoleName AS Role,
                   u.IsActive
            FROM Users u
            INNER JOIN Roles r ON u.RoleId = r.RoleId
            WHERE u.Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT u.*,r.RoleName as Role FROM Users u inner join Roles R\r\non u.roleId=R.RoleId WHERE u.Email = @Email;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        return result;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
    {
        const string sql = @"
            SELECT u.Id,
                   u.FullName,
                   u.Email,
                   u.PasswordHash,
                   u.Mobile,
                   u.RoleId,
                   r.RoleName AS Role,
                   u.IsActive
            FROM Users u
            INNER JOIN Roles r ON u.RoleId = r.RoleId
            WHERE (@IncludeInactive = 1) OR (u.IsActive = 1)
            ORDER BY u.FullName;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<User>(sql, new { IncludeInactive = includeInactive });
    }
}
