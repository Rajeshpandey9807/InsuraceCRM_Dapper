using System.Collections.Generic;
using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> InsertAsync(Customer customer)
    {
        const string sql = @"
            INSERT INTO Customers (
                Name,
                MobileNumber,
                Location,
                InsuranceType,
                Income,
                SourceOfIncome,
                FamilyMembers,
                AssignedEmployeeId,
                CreatedDate)
            VALUES (
                @Name,
                @MobileNumber,
                @Location,
                @InsuranceType,
                @Income,
                @SourceOfIncome,
                @FamilyMembers,
                @AssignedEmployeeId,
                @CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, customer);
    }

    public async Task UpdateAsync(Customer customer)
    {
        const string sql = @"
            UPDATE Customers
            SET Name = @Name,
                MobileNumber = @MobileNumber,
                Location = @Location,
                InsuranceType = @InsuranceType,
                Income = @Income,
                SourceOfIncome = @SourceOfIncome,
                FamilyMembers = @FamilyMembers,
                AssignedEmployeeId = @AssignedEmployeeId
            WHERE Id = @Id;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, customer);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Customers WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM Customers WHERE Id = @Id;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Customers ORDER BY CreatedDate DESC;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Customer>(sql);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByEmployeeAsync(int employeeId)
    {
        const string sql = @"
            SELECT * FROM Customers
            WHERE AssignedEmployeeId = @EmployeeId
            ORDER BY CreatedDate DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Customer>(sql, new { EmployeeId = employeeId });
    }

    public async Task AssignCustomerAsync(int customerId, int employeeId)
    {
        const string sql = @"
            UPDATE Customers
            SET AssignedEmployeeId = @EmployeeId
            WHERE Id = @CustomerId;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { EmployeeId = employeeId, CustomerId = customerId });
    }

    public async Task AssignCustomersAsync(IEnumerable<int> customerIds, int employeeId)
    {
        const string sql = @"
            UPDATE Customers
            SET AssignedEmployeeId = @EmployeeId
            WHERE Id IN @CustomerIds;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { EmployeeId = employeeId, CustomerIds = customerIds });
    }
}
