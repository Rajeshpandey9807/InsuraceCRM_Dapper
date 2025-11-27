using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name, Description, IsSystem FROM Roles ORDER BY Name;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Role>(sql);
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, Name, Description, IsSystem FROM Roles WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { Id = id });
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        const string sql = @"
            SELECT TOP 1 Id, Name, Description, IsSystem
            FROM Roles
            WHERE LOWER(Name) = LOWER(@Name);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { Name = name });
    }

    public async Task<int> InsertAsync(Role role)
    {
        const string sql = @"
            INSERT INTO Roles (Name, Description, IsSystem)
            VALUES (@Name, @Description, @IsSystem);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, role);
    }

    public async Task UpdateAsync(Role role)
    {
        const string sql = @"
            UPDATE Roles
            SET Name = @Name,
                Description = @Description,
                IsSystem = @IsSystem
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, role);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Roles WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
