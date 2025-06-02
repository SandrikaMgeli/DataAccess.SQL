using System.Data;

namespace DataAccess.SQL.Abstraction;

public interface IDbManager
{
    Task<IAsyncDbContext> RunWithTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    IDbContext RunWithTransaction(IsolationLevel isolationLevel);
}