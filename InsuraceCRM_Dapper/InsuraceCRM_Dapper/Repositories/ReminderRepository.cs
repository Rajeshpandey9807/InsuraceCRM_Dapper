using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class ReminderRepository : IReminderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReminderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(Reminder reminder)
    {
        const string sql = @"
            INSERT INTO Reminders (CustomerId, EmployeeId, ReminderDateTime, Note, IsShown)
            VALUES (@CustomerId, @EmployeeId, @ReminderDateTime, @Note, @IsShown);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, reminder);
    }

    public async Task UpdateAsync(Reminder reminder)
    {
        const string sql = @"
            UPDATE Reminders
            SET CustomerId = @CustomerId,
                EmployeeId = @EmployeeId,
                ReminderDateTime = @ReminderDateTime,
                Note = @Note,
                IsShown = @IsShown
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, reminder);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Reminders WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Reminder?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT r.*, c.Name AS CustomerName, c.MobileNumber AS CustomerMobileNumber
            FROM Reminders r
            LEFT JOIN Customers c ON c.Id = r.CustomerId
            WHERE r.Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Reminder>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Reminder>> GetByCustomerIdAsync(int customerId)
    {
        const string sql = @"
            SELECT r.*, c.Name AS CustomerName, c.MobileNumber AS CustomerMobileNumber
            FROM Reminders r
            LEFT JOIN Customers c ON c.Id = r.CustomerId
            WHERE r.CustomerId = @CustomerId
            ORDER BY r.ReminderDateTime DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Reminder>(sql, new { CustomerId = customerId });
    }

    public async Task<IEnumerable<Reminder>> GetDueRemindersAsync(int employeeId)
    {
        const string sql = @"
            SELECT r.*, c.Name AS CustomerName, c.MobileNumber AS CustomerMobileNumber
            FROM Reminders r
            LEFT JOIN Customers c ON c.Id = r.CustomerId
            WHERE r.EmployeeId = @EmployeeId
              AND r.ReminderDateTime <= SYSUTCDATETIME()
              AND r.IsShown = 0
            ORDER BY r.ReminderDateTime;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Reminder>(sql, new { EmployeeId = employeeId });
    }

    public async Task MarkReminderShownAsync(int reminderId)
    {
        const string sql = @"
            UPDATE Reminders
            SET IsShown = 1
            WHERE Id = @ReminderId;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { ReminderId = reminderId });
    }
}
