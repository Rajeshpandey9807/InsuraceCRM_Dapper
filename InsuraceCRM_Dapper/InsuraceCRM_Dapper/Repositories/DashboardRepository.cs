using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DashboardRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Reminder>> GetTodaysRemindersAsync(int? employeeId)
    {
        const string sql = @"
             SELECT c.*, c.Name AS CustomerName, c.MobileNumber AS CustomerMobileNumber
 from FollowUps f
 LEFT JOIN Customers c ON c.Id = f.CustomerId
             WHERE CONVERT(date, f.NextReminderDateTime) = CONVERT(date, SYSUTCDATETIME())
            ORDER BY f.NextReminderDateTime;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Reminder>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetTodaysCallCountAsync(int? employeeId)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM FollowUps f
            INNER JOIN Customers c ON c.Id = f.CustomerId
            WHERE f.FollowUpDate = CONVERT(date, SYSUTCDATETIME())
              AND (@EmployeeId IS NULL OR c.AssignedEmployeeId = @EmployeeId)";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetAssignedCustomerCountAsync(int? employeeId)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM Customers
            WHERE AssignedEmployeeId IS NOT NULL
              AND (@EmployeeId IS NULL OR AssignedEmployeeId = @EmployeeId)";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetTotalCustomerCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Customers;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
