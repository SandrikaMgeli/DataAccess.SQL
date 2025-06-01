using System.Data;
using DataAccess.SQL.Abstraction;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlAsyncDbContext : IAsyncDbContext
{
    public IDbConnection? Connection { get; }
    public IDbTransaction? Transaction { get; }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}