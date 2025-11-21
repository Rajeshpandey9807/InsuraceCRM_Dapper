using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class FollowUpRepository : IFollowUpRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FollowUpRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(FollowUp followUp)
    {
        const string sql = @"
            INSERT INTO FollowUps (CustomerId, FollowUpDate, FollowUpNote, FollowUpStatus, NextReminderDateTime)
            VALUES (@CustomerId, @FollowUpDate, @FollowUpNote, @FollowUpStatus, @NextReminderDateTime);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, followUp);
    }

    public async Task UpdateAsync(FollowUp followUp)
    {
        const string sql = @"
            UPDATE FollowUps
            SET FollowUpDate = @FollowUpDate,
                FollowUpNote = @FollowUpNote,
                FollowUpStatus = @FollowUpStatus,
                NextReminderDateTime = @NextReminderDateTime
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, followUp);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM FollowUps WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<FollowUp?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM FollowUps WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<FollowUp>(sql, new { Id = id });
    }

    public async Task<IEnumerable<FollowUp>> GetByCustomerIdAsync(int customerId)
    {
        const string sql = @"
            SELECT * FROM FollowUps
            WHERE CustomerId = @CustomerId
            ORDER BY FollowUpDate DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<FollowUp>(sql, new { CustomerId = customerId });
    }
}
