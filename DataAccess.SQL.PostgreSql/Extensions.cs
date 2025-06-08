using DataAccess.SQL.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public static class Extensions
{
    public static IServiceCollection AddPostgreSql(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<PostgreSqlDbContext>();

        services.AddScoped<IDbManager, PostgreSqlDbManager>();

        services.AddSingleton<DbOptions>(
            new DbOptions()
            {
                ConnectionString = connectionString,
            });

        return services;
    }
}