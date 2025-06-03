using System.Data;
using DataAccess.SQL.Abstraction;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbContext(IOptions<NpgsqlConnectionStringBuilder> connectionStringBuilder) : IDbContext, IAsyncDbContext
{
    private DbManagerState _state =  DbManagerState.None;

    private NpgsqlConnection? Connection { get; set; }

    private NpgsqlTransaction? Transaction { get; set; }

    public static implicit operator NpgsqlConnection(PostgreSqlDbContext context)
    {
        return (context._state switch
        {
            DbManagerState.None => context.GetConnectionFromPool(),
            DbManagerState.Transactional => context.Connection,
            _ => throw new InvalidCastException($"{nameof(PostgreSqlDbContext)} is in unknown state")
        })!;
    }

    public static implicit operator NpgsqlTransaction?(PostgreSqlDbContext context)
    {
        return context.Transaction;
    }

    public async Task SetupTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        Connection = GetConnectionFromPool();
        Transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public void SetupTransaction(IsolationLevel isolationLevel)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        Connection = GetConnectionFromPool();
        Transaction = Connection.BeginTransaction(isolationLevel);
    }

    private void EnsureNotInTransactionalState()
    {
        if (_state == DbManagerState.Transactional)
        {
            throw new InvalidOperationException($"{nameof(PostgreSqlDbContext)} was already in transactional state");
        }
    }

    private NpgsqlConnection GetConnectionFromPool()
    {
        NpgsqlConnection connection = new NpgsqlConnection(connectionStringBuilder.Value.ConnectionString);
        connection.Open();
        return connection;
    }

    public void Dispose()
    {
        if (Connection != null) Connection.Dispose();
        if (Transaction != null) Transaction.Dispose();
        Reset();
    }

    public async ValueTask DisposeAsync()
    {
        if (Connection != null) await Connection.DisposeAsync();
        if (Transaction != null) await Transaction.DisposeAsync();
        Reset();
    }

    private void Reset()
    {
        Connection = null;
        Transaction = null;
        _state = DbManagerState.None;
    }
}