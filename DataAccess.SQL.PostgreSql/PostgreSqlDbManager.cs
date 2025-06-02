using System.Data;
using DataAccess.SQL.Abstraction;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbManager(PostgreSqlDbContext dbContext) : IDbManager
{
    public async Task<IAsyncDbContext> RunWithTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        await dbContext.SetupTransactionAsync(isolationLevel, cancellationToken);
        return dbContext;
    }

    public IDbContext RunWithTransaction(IsolationLevel isolationLevel)
    {
        dbContext.SetupTransaction(isolationLevel);
        return dbContext;
    }
}