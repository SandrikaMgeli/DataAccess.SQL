namespace DataAccess.SQL.Abstraction;

public interface IAsyncDbContext : IAsyncDisposable
{
    Task<T> QuerySingleAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    Task<T> QueryFirstAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? @params = null, CancellationToken cancellationToken = default);

    Task<int> ExecuteAsync(string sql, object? @params = null, CancellationToken cancellationToken = default);
}