using System.Data;
using DataAccess.SQL.Abstraction;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbManager : IDbManager
{
    private DbManagerState _dbManagerState =  DbManagerState.None;


    public Task<IAsyncDbContext> RunWithTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        _dbManagerState = DbManagerState.Transactional;
        throw new NotImplementedException();
    }

    public Task<IDbContext> RunWithTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }
}