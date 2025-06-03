using System.Data;
using Dapper;
using DataAccess.SQL.Abstraction;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbContext(IOptions<NpgsqlConnectionStringBuilder> connectionStringBuilder) : IDbContext, IAsyncDbContext
{
    private DbManagerState _state =  DbManagerState.None;

    private NpgsqlConnection? _connection;

    private NpgsqlConnection Connection =>
        _state switch
        {
            DbManagerState.None => GetConnectionFromPool(),
            DbManagerState.Transactional => _connection!,
            _ => throw new InvalidCastException($"{nameof(PostgreSqlDbContext)} is in unknown state")
        };

    private NpgsqlTransaction? _transaction;

    private NpgsqlTransaction? Transaction =>
        _state switch
        {
            DbManagerState.None => null,
            DbManagerState.Transactional => _transaction,
            _ => throw new InvalidCastException($"{nameof(PostgreSqlDbContext)} is in unknown state")
        };

    public async Task SetupTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        _connection = GetConnectionFromPool();
        _transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public void SetupTransaction(IsolationLevel isolationLevel)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        _connection = GetConnectionFromPool();
        _transaction = Connection.BeginTransaction(isolationLevel);
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
        if (_connection != null) _connection.Dispose();
        if (_transaction != null) _transaction.Dispose();
        Reset();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null) await _connection.DisposeAsync();
        if (_transaction != null) await _transaction.DisposeAsync();
        Reset();
    }

    private void Reset()
    {
        _connection = null;
        _transaction = null;
        _state = DbManagerState.None;
    }

    public T QuerySingle<T>(string sql, object @params)
    {
        return Connection.QuerySingle<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public T? QuerySingleOrDefault<T>(string sql, object @params)
    {
        return Connection.QuerySingleOrDefault<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public IEnumerable<T> Query<T>(string sql, object @params)
    {
        return Connection.Query<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public T QueryFirst<T>(string sql, object @params)
    {
        return Connection.QueryFirst<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public T? QueryFirstOrDefault<T>(string sql, object @params)
    {
        return Connection.QueryFirstOrDefault<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public int Execute(string sql, object @params)
    {
        return Connection.Execute(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<T> QuerySingleAsync<T>(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.QuerySingleAsync<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.QuerySingleOrDefaultAsync<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.QueryAsync<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<T> QueryFirstAsync<T>(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.QueryFirstAsync<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.QueryFirstOrDefaultAsync<T>(sql: sql,  transaction: Transaction, param: @params);
    }

    public Task<int> ExecuteAsync(string sql, object @params, CancellationToken cancellationToken)
    {
        return Connection.ExecuteAsync(sql: sql,  transaction: Transaction, param: @params);
    }
}