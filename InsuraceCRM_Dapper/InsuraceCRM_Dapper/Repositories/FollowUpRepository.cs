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
            INSERT INTO FollowUps (
                CustomerId,
                FollowUpDate,
                InsuranceType,
                Budget,
                HasExistingPolicy,
                FollowUpNote,
                FollowUpStatus,
                NextReminderDateTime,
                ReminderRequired,
                IsConverted,
                ConversionReason,
                SoldProductName,
                TicketSize,
                PolicyNumber,
                PolicyEnforceDate)
            VALUES (
                @CustomerId,
                @FollowUpDate,
                @InsuranceType,
                @Budget,
                @HasExistingPolicy,
                @FollowUpNote,
                @FollowUpStatus,
                @NextReminderDateTime,
                @ReminderRequired,
                @IsConverted,
                @ConversionReason,
                @SoldProductName,
                @TicketSize,
                @PolicyNumber,
                @PolicyEnforceDate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, followUp);
    }

    public async Task UpdateAsync(FollowUp followUp)
    {
        const string sql = @"
            UPDATE FollowUps
            SET FollowUpDate = @FollowUpDate,
                InsuranceType = @InsuranceType,
                Budget = @Budget,
                HasExistingPolicy = @HasExistingPolicy,
                FollowUpNote = @FollowUpNote,
                FollowUpStatus = @FollowUpStatus,
                NextReminderDateTime = @NextReminderDateTime,
                ReminderRequired = @ReminderRequired,
                IsConverted = @IsConverted,
                ConversionReason = @ConversionReason,
                SoldProductName = @SoldProductName,
                TicketSize = @TicketSize,
                PolicyNumber = @PolicyNumber,
                PolicyEnforceDate = @PolicyEnforceDate
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
