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
        const string sql = "select RoleId,RoleName from Roles";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Role>(sql).ContinueWith(t => t.Result.ToList());
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
        const string sql = "UPDATE Users SET RoleId = @role WHERE Id = @UserId;";
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
        const string sql = "SELECT * FROM Users WHERE Id = @Id;";
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
            SELECT *
            FROM Users
            WHERE IsActive = 1
            ORDER BY FullName;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<User>(sql, new { IncludeInactive = includeInactive });
    }
}
