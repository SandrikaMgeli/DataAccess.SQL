using System.Data;
using Dapper;
using DataAccess.SQL.Abstraction;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public class PostgreSqlDbContext(DbOptions dbOptions) : DbContext
{
    private DbManagerState _state =  DbManagerState.None;

    private NpgsqlConnection? _connection;

    private NpgsqlTransaction? _transaction;

    public async Task SetupTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        _connection = GetConnectionFromPool();
        _transaction = await _connection.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public void SetupTransaction(IsolationLevel isolationLevel)
    {
        EnsureNotInTransactionalState();
        _state = DbManagerState.Transactional;
        _connection = GetConnectionFromPool();
        _transaction = _connection.BeginTransaction(isolationLevel);
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
        NpgsqlConnection connection = new NpgsqlConnection(dbOptions.ConnectionString);
        connection.Open();
        return connection;
    }

    public sealed override void Dispose()
    {
        if (_connection != null) _connection.Dispose();
        if (_transaction != null) _transaction.Dispose();
        Reset();
    }

    public sealed override async ValueTask DisposeAsync()
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

    public sealed override T QuerySingle<T>(string sql, object? @params = null)
    {
        return Run(conn => conn.QuerySingle<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public override T? QuerySingleOrDefault<T>(string sql, object? @params = null) where T : default
    {
        return Run(conn => conn.QuerySingleOrDefault<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override IEnumerable<T> Query<T>(string sql, object? @params = null)
    {
        return Run(conn => conn.Query<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override T QueryFirst<T>(string sql, object? @params = null)
    {
        return Run(conn => conn.QueryFirst<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override T? QueryFirstOrDefault<T>(string sql, object? @params = null) where T : default
    {
        return Run(conn => conn.QueryFirstOrDefault<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override int Execute(string sql, object? @params = null)
    {
        return Run(conn => conn.Execute(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override async Task<T> QuerySingleAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default)
    {
        return await RunAsync(conn => conn.QuerySingleAsync<T>(sql: sql, transaction: _transaction, param: @params));
    }

    public sealed override async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default) where T : default
    {
        return await RunAsync(conn => conn.QuerySingleOrDefaultAsync<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default)
    {
        return await RunAsync(conn => conn.QueryAsync<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override async Task<T> QueryFirstAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default)
    {
        return await RunAsync(conn => conn.QueryFirstAsync<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default) where T : default
    {
        return await RunAsync(conn => conn.QueryFirstOrDefaultAsync<T>(sql: sql,  transaction: _transaction, param: @params));
    }

    public sealed override async Task<int> ExecuteAsync(string sql, object? @params = null, CancellationToken cancellationToken = default)
    {
        return await RunAsync(conn => conn.ExecuteAsync(sql: sql,  transaction: _transaction, param: @params));
    }

    private async Task<T> RunAsync<T>(Func<NpgsqlConnection, Task<T>> target)
    {
        if (_state == DbManagerState.None)
        {
            await using NpgsqlConnection connection = GetConnectionFromPool();
            T response = await target(connection);
            return response;
        }

        if (_state == DbManagerState.Transactional)
        {
            T response = await target(_connection!);
            return response;
        }

        throw new InvalidOperationException($"{nameof(PostgreSqlDbContext)} was in unknown {nameof(DbManagerState)}");
    }

    private T Run<T>(Func<NpgsqlConnection, T> target)
    {
        if (_state == DbManagerState.None)
        {
            using NpgsqlConnection connection = GetConnectionFromPool();
            T response = target(connection);
            return response;
        }

        if (_state == DbManagerState.Transactional)
        {
            T response = target(_connection!);
            return response;
        }

        throw new InvalidOperationException($"{nameof(PostgreSqlDbContext)} was in unknown {nameof(DbManagerState)}");
    }
}