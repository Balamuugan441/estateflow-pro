using System.Data;

namespace Brokerage.Data.Database;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}