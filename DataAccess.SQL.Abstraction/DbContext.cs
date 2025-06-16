namespace DataAccess.SQL.Abstraction;

public abstract class DbContext : IDbContext, IAsyncDbContext
{
    public abstract void Dispose();

    public abstract T QuerySingle<T>(string sql, object? @params = null);

    public abstract T? QuerySingleOrDefault<T>(string sql, object? @params = null);

    public abstract IEnumerable<T> Query<T>(string sql, object? @params = null);

    public abstract T QueryFirst<T>(string sql, object? @params = null);

    public abstract T? QueryFirstOrDefault<T>(string sql, object? @params = null);

    public abstract int Execute(string sql, object? @params = null);

    public abstract ValueTask DisposeAsync();

    public abstract Task<T> QuerySingleAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    public abstract Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    public abstract Task<IEnumerable<T>> QueryAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    public abstract Task<T> QueryFirstAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    public abstract Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    public abstract Task<int> ExecuteAsync(string sql, object? @params = null, CancellationToken cancellationToken = default);
}