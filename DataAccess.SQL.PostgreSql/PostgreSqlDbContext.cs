using System.Data;
using DataAccess.SQL.Abstraction;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbContext : IDbContext
{
    public IDbConnection? Connection { get; }
    public IDbTransaction? Transaction { get; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}