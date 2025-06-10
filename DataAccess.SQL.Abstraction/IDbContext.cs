namespace DataAccess.SQL.Abstraction;

public interface IDbContext : IDisposable
{
    T QuerySingle<T>(string sql, object? @params = null);

    T? QuerySingleOrDefault<T>(string sql, object? @params = null);

    IEnumerable<T> Query<T>(string sql, object? @params = null);

    T QueryFirst<T>(string sql, object? @params = null);

    T? QueryFirstOrDefault<T>(string sql, object? @params = null);

    int Execute(string sql, object? @params = null);
}