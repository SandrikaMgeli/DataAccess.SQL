using System.Data;

namespace DataAccess.SQL.Abstraction;

public interface IDbContext : IDisposable
{
    T QuerySingle<T>(string sql, IDbTransaction? transaction, object @params);

    T? QuerySingleOrDefault<T>(string sql, IDbTransaction? transaction, object @params);

    IEnumerable<T> Query<T>(string sql, IDbTransaction? transaction, object @params);

    T QueryFirst<T>(string sql, IDbTransaction? transaction, object @params);

    T? QueryFirstOrDefault<T>(string sql, IDbTransaction? transaction, object @params);
}