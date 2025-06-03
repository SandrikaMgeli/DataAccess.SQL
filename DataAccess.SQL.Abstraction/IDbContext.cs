using System.Data;

namespace DataAccess.SQL.Abstraction;

public interface IDbContext : IDisposable
{
    T QuerySingle<T>(string sql, object @params);

    T? QuerySingleOrDefault<T>(string sql, object @params);

    IEnumerable<T> Query<T>(string sql, object @params);

    T QueryFirst<T>(string sql, object @params);

    T? QueryFirstOrDefault<T>(string sql, object @params);

    int Execute(string sql, object @params);
}