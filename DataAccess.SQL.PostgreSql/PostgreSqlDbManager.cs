using System.Data;
using DataAccess.SQL.Abstraction;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbManager(DbContext dbContext) : IDbManager
{
    public async Task<IAsyncDbContext> RunWithTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        await ((PostgreSqlDbContext)dbContext).SetupTransactionAsync(isolationLevel, cancellationToken);
        return dbContext;
    }

    public IDbContext RunWithTransaction(IsolationLevel isolationLevel)
    {
        ((PostgreSqlDbContext)dbContext).SetupTransaction(isolationLevel);
        return dbContext;
    }
}