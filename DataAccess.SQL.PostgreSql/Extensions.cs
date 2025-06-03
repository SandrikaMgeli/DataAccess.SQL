using DataAccess.SQL.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public static class Extensions
{
    public static IServiceCollection AddPostgreSql(this IServiceCollection services, string configurationToBind)
    {
        services.AddScoped<PostgreSqlDbContext>();

        services.AddScoped<IDbManager, PostgreSqlDbManager>();

        services.AddOptions<NpgsqlConnectionStringBuilder>()
            .BindConfiguration(configurationToBind);

        return services;
    }
}