using System.Data;

namespace DataAccess.SQL.Abstraction;

public interface IDbManager
{
    Task<IAsyncDbContext> RunWithTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    Task<IDbContext> RunWithTransaction(IsolationLevel isolationLevel);
}