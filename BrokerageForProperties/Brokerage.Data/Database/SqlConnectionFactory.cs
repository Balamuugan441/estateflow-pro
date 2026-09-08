using System.Data;
using Microsoft.Data.SqlClient;

namespace Brokerage.Data.Database;
// Gets the connection string from the appsettings.json file and creates a new SqlConnection object.
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
