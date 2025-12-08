namespace InsuraceCRM_Dapper.Data;

public interface IDatabaseMigrator
{
    Task EnsureSchemaAsync();
}
