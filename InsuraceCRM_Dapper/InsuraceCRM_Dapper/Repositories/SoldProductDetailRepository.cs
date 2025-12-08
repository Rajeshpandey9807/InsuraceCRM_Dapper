using Dapper;
using InsuraceCRM_Dapper.Data;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Repositories;

public class SoldProductDetailRepository : ISoldProductDetailRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SoldProductDetailRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> UpsertAsync(SoldProductDetail detail)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM SoldProductDetails WHERE FollowUpId = @FollowUpId)
BEGIN
    UPDATE SoldProductDetails
    SET CustomerId = @CustomerId,
        SoldProductId = @SoldProductId,
        SoldProductName = @SoldProductName,
        TicketSize = @TicketSize,
        TenureInYears = @TenureInYears,
        PolicyNumber = @PolicyNumber,
        PolicyEnforceDate = @PolicyEnforceDate,
        UpdatedOn = SYSUTCDATETIME()
    WHERE FollowUpId = @FollowUpId;

    SELECT Id FROM SoldProductDetails WHERE FollowUpId = @FollowUpId;
END
ELSE
BEGIN
    INSERT INTO SoldProductDetails (
        CustomerId,
        FollowUpId,
        SoldProductId,
        SoldProductName,
        TicketSize,
        TenureInYears,
        PolicyNumber,
        PolicyEnforceDate)
    VALUES (
        @CustomerId,
        @FollowUpId,
        @SoldProductId,
        @SoldProductName,
        @TicketSize,
        @TenureInYears,
        @PolicyNumber,
        @PolicyEnforceDate);

    SELECT CAST(SCOPE_IDENTITY() as int);
END";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, detail);
    }

    public async Task DeleteByFollowUpIdAsync(int followUpId)
    {
        const string sql = "DELETE FROM SoldProductDetails WHERE FollowUpId = @FollowUpId;";
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { FollowUpId = followUpId });
    }

    public async Task<IEnumerable<SoldProductDetail>> GetByCustomerIdAsync(int customerId)
    {
        const string sql = @"
            SELECT *
            FROM SoldProductDetails
            WHERE CustomerId = @CustomerId
            ORDER BY PolicyEnforceDate DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<SoldProductDetail>(sql, new { CustomerId = customerId });
    }

    public async Task<IEnumerable<SoldProductDetailInfo>> GetAllWithDetailsAsync()
    {
        const string sql = @"
            SELECT sp.Id,
                   sp.CustomerId,
                   c.Name AS CustomerName,
                   c.MobileNumber AS CustomerMobileNumber,
                   c.Location AS CustomerLocation,
                   sp.FollowUpId,
                   f.FollowUpDate,
                   sp.SoldProductId AS ProductId,
                   p.Name AS ProductName,
                   sp.SoldProductName,
                   sp.TicketSize,
                   sp.TenureInYears,
                   sp.PolicyNumber,
                   sp.PolicyEnforceDate,
                   sp.CreatedOn,
                   sp.UpdatedOn
            FROM SoldProductDetails sp
            INNER JOIN Customers c ON c.Id = sp.CustomerId
            INNER JOIN Products p ON p.Id = sp.SoldProductId
            LEFT JOIN FollowUps f ON f.Id = sp.FollowUpId
            ORDER BY sp.PolicyEnforceDate DESC, sp.Id DESC;";

        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<SoldProductDetailInfo>(sql);
    }
}
