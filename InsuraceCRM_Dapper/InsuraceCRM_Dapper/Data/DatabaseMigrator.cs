using Dapper;
using Microsoft.Extensions.Logging;

namespace InsuraceCRM_Dapper.Data;

public class DatabaseMigrator : IDatabaseMigrator
{
    private const string DefaultSchema = "dbo";

    private static readonly IReadOnlyList<ColumnDefinition> ColumnDefinitions =
    [
        new(DefaultSchema, "Customers", "InsuranceType",
            "ALTER TABLE dbo.Customers ADD InsuranceType NVARCHAR(100) NULL;"),
        new(DefaultSchema, "FollowUps", "InsuranceType",
            "ALTER TABLE dbo.FollowUps ADD InsuranceType NVARCHAR(100) NULL;"),
        new(DefaultSchema, "FollowUps", "Budget",
            "ALTER TABLE dbo.FollowUps ADD Budget DECIMAL(18,2) NULL;"),
        new(DefaultSchema, "FollowUps", "HasExistingPolicy",
            "ALTER TABLE dbo.FollowUps ADD HasExistingPolicy BIT NOT NULL CONSTRAINT DF_FollowUps_HasExistingPolicy DEFAULT 0 WITH VALUES;"),
        new(DefaultSchema, "FollowUps", "ReminderRequired",
            "ALTER TABLE dbo.FollowUps ADD ReminderRequired BIT NOT NULL CONSTRAINT DF_FollowUps_ReminderRequired DEFAULT 0 WITH VALUES;"),
        new(DefaultSchema, "FollowUps", "IsConverted",
            "ALTER TABLE dbo.FollowUps ADD IsConverted BIT NULL;"),
        new(DefaultSchema, "FollowUps", "ConversionReason",
            "ALTER TABLE dbo.FollowUps ADD ConversionReason NVARCHAR(500) NULL;")
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IDbConnectionFactory connectionFactory, ILogger<DatabaseMigrator> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task EnsureSchemaAsync()
    {
        const string columnExistsSql = @"
SELECT COUNT(*) 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @SchemaName
  AND TABLE_NAME = @TableName
  AND COLUMN_NAME = @ColumnName;";

        using var connection = await _connectionFactory.CreateConnectionAsync();

        foreach (var column in ColumnDefinitions)
        {
            var columnExists = await connection.ExecuteScalarAsync<int>(
                columnExistsSql,
                new
                {
                    column.SchemaName,
                    column.TableName,
                    column.ColumnName
                });

            if (columnExists > 0)
            {
                continue;
            }

            _logger.LogInformation("Adding missing column {Table}.{Column}", column.TableName, column.ColumnName);
            await connection.ExecuteAsync(column.AddColumnSql);
        }
    }

    private sealed record ColumnDefinition(
        string SchemaName,
        string TableName,
        string ColumnName,
        string AddColumnSql);
}
