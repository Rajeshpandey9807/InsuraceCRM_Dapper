using System.Data;

namespace InsuraceCRM_Dapper.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}
