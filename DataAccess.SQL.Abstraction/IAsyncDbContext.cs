using System.Data;

namespace DataAccess.SQL.Abstraction;

public interface IAsyncDbContext : IAsyncDisposable
{
    Task<T> QuerySingleAsync<T>(string sql, object @params, CancellationToken cancellationToken);

    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object @params, CancellationToken cancellationToken);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object @params, CancellationToken cancellationToken);

    Task<T> QueryFirstAsync<T>(string sql, object @params, CancellationToken cancellationToken);

    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object @params, CancellationToken cancellationToken);
}