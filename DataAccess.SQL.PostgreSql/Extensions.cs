using DataAccess.SQL.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataAccess.SQL.PostgreSql;

public static class Extensions
{
    public static IServiceCollection AddPostgreSql(this IServiceCollection services, string connectionString, Action<DbOptions>? dbOptionsAction = null)
    {
        services.AddScoped<PostgreSqlDbContext>();

        services.AddScoped<IDbManager, PostgreSqlDbManager>();

        DbOptions dbOptions = new DbOptions(connectionString);
        dbOptionsAction?.Invoke(dbOptions);
        services.AddSingleton<DbOptions>(dbOptions);

        return services;
    }

    public static IHost SetupPostgreSql(this IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        DbOptions dbOptions = serviceScope.ServiceProvider.GetRequiredService<DbOptions>();
        IDbManager dbManager = serviceScope.ServiceProvider.GetRequiredService<IDbManager>();
        foreach (string migrationFilePath in dbOptions.MigrationFilePaths)
        {
            ProcessMigration(migrationFilePath, dbManager);
        }

        return host;
    }

    private static void ProcessMigration(string migrationFilePath, IDbManager dbManager)
    {
        // TODO: implement migration logic
    }
}