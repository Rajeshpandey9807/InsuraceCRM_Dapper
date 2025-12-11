using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class FollowUpRepository : IFollowUpRepository
{
    private const string NormalizedIsConvertedProjection = @"
        CASE
            WHEN TRY_CAST(f.IsConverted AS bit) IS NOT NULL THEN TRY_CAST(f.IsConverted AS bit)
            WHEN LTRIM(RTRIM(LOWER(CAST(f.IsConverted AS NVARCHAR(10))))) IN ('true', 'yes') THEN CAST(1 AS bit)
            WHEN LTRIM(RTRIM(LOWER(CAST(f.IsConverted AS NVARCHAR(10))))) IN ('false', 'no') THEN CAST(0 AS bit)
            ELSE NULL
        END";
    private readonly IDbConnectionFactory _connectionFactory;

    public FollowUpRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string BuildFollowUpSelectColumns() => $@"
        f.Id,
        f.CustomerId,
        f.FollowUpDate,
        f.InsuranceType,
        f.Budget,
        f.HasExistingPolicy,
        f.FollowUpNote,
        f.FollowUpStatus,
        f.NextReminderDateTime,
        f.ReminderRequired,
        {NormalizedIsConvertedProjection} AS IsConverted,
        f.ConversionReason,
        f.CreatedBy";

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
                CreatedBy)
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
                @CreatedBy);
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
                ConversionReason = @ConversionReason
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
        var sql = $@"
            SELECT
                {BuildFollowUpSelectColumns()},
                sp.SoldProductId,
                sp.SoldProductName,
                sp.TicketSize,
                sp.TenureInYears,
                sp.PolicyNumber,
                sp.PolicyEnforceDate
            FROM FollowUps f
            LEFT JOIN SoldProductDetails sp ON sp.FollowUpId = f.Id
            WHERE f.Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<FollowUp>(sql, new { Id = id });
    }

    public async Task<IEnumerable<FollowUp>> GetByCustomerIdAsync(int customerId)
    {
        var sql = $@"
            SELECT
                {BuildFollowUpSelectColumns()},
                sp.SoldProductId,
                sp.SoldProductName,
                sp.TicketSize,
                sp.TenureInYears,
                sp.PolicyNumber,
                sp.PolicyEnforceDate
            FROM FollowUps f
            LEFT JOIN SoldProductDetails sp ON sp.FollowUpId = f.Id
            WHERE f.CustomerId = @CustomerId
            ORDER BY f.FollowUpDate DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<FollowUp>(sql, new { CustomerId = customerId });
    }

    public async Task<IEnumerable<UserFollowUpDetail>> GetByEmployeeIdAsync(int employeeId)
    {
        var sql = $@"
            SELECT f.Id AS FollowUpId,
                   f.CustomerId,
                   c.Name AS CustomerName,
                   c.MobileNumber AS CustomerMobileNumber,
                   c.Location AS CustomerLocation,
                   f.FollowUpDate,
                   f.InsuranceType,
                   f.Budget,
                   f.HasExistingPolicy,
                   f.FollowUpStatus,
                   f.FollowUpNote,
                   f.NextReminderDateTime,
                   f.ReminderRequired,
                   {NormalizedIsConvertedProjection} AS IsConverted,
                   sp.SoldProductName,
                   sp.TicketSize,
                   sp.TenureInYears,
                   sp.PolicyNumber,
                   sp.PolicyEnforceDate
            FROM FollowUps f
            INNER JOIN Customers c ON c.Id = f.CustomerId
            LEFT JOIN SoldProductDetails sp ON sp.FollowUpId = f.Id
            WHERE f.CreatedBy = @EmployeeId
            ORDER BY f.FollowUpDate DESC, f.Id DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<UserFollowUpDetail>(sql, new { EmployeeId = employeeId });
    }
}
