using Dapper;
using DataAccess.SQL.Abstraction;
using Medallion.Threading.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DataAccess.SQL.PostgreSql;

public static class Extensions
{
    private static readonly List<string> LibraryDbInitializer =
    [
        @"CREATE TABLE IF NOT EXISTS applied_migrations
        (
            id SERIAL PRIMARY KEY,
            name VARCHAR(200) UNIQUE NOT NULL
        );"
    ];
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
        IDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();
        ILogger<IDbManager> logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<IDbManager>>();
        EnsureDatabaseExists(dbOptions.ConnectionString, logger);

        foreach (string migrationFilePath in dbOptions.MigrationFilePaths)
        {
            ProcessMigration(migrationFilePath, dbOptions.ConnectionString, dbContext);
        }

        return host;
    }

    private static void ProcessMigration(string migrationFilePath, string connectionString, IDbContext dbContext)
    {
        string migrationName = Path.GetFileNameWithoutExtension(migrationFilePath);
        PostgresDistributedLock migrationDistributedLock = new(new PostgresAdvisoryLockKey(migrationName, allowHashing: true), connectionString);
        using (migrationDistributedLock.Acquire())
        {
            ProcessMigration(migrationFilePath, dbContext, migrationName);
        }
    }

    private static void ProcessMigration(string migrationFilePath, IDbContext dbContext, string migrationName)
    {
        bool isMigrationApplied = dbContext.QuerySingleOrDefault<bool>("SELECT TRUE FROM applied_migrations WHERE name = @name", new { name = migrationName });
        if (isMigrationApplied)
        {
            return;
        }

        string migrationSql = File.ReadAllText(migrationFilePath);
        dbContext.Execute(migrationSql);
        dbContext.Execute("INSERT INTO applied_migrations (name) VALUES (@name)", new { name = migrationName });
    }

    private static void EnsureDatabaseExists(string connectionString, ILogger<IDbManager> logger)
    {
        string targetDatabase = GetDatabaseNameToCreate(connectionString) ?? throw new InvalidOperationException("There is not database specified in connection string");

        string adminConnectionString = CreateAdminConnectionString(connectionString);

        try
        {
            EnsureDatabaseExists(logger, adminConnectionString, targetDatabase);
            EnsureLibraryDatabaseInitialized(connectionString, logger);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to ensure database '{targetDatabase}' exists: {ex.Message}", ex);
        }
    }

    private static void EnsureLibraryDatabaseInitialized(string connectionString, ILogger<IDbManager> logger)
    {
        using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        connection.Open();

        foreach (string dbInitializerSql in LibraryDbInitializer)
        {
            connection.Execute(dbInitializerSql);
        }
    }

    private static void EnsureDatabaseExists(ILogger<IDbManager> logger, string adminConnectionString, string targetDatabase)
    {
        using NpgsqlConnection adminConnection = new NpgsqlConnection(adminConnectionString);
        adminConnection.Open();

        // Check if target database exists
        bool targetDbExists = adminConnection.QuerySingleOrDefault<bool>("SELECT TRUE FROM pg_database WHERE datname = @targetDatabase;",  new { targetDatabase });

        if (!targetDbExists) // If database doesn't exist, create it
        {
            string createTargetDbSql = $"CREATE DATABASE \"{targetDatabase}\"";
            adminConnection.Execute(createTargetDbSql);
            
            logger.LogInformation($"Database '{targetDatabase}' created successfully.");
        }
    }

    private static string CreateAdminConnectionString(string connectionString)
    {
        // Create connection string to connect to 'postgres' database for admin operations
        NpgsqlConnectionStringBuilder adminBuilder = new(connectionString)
        {
            Database = "postgres" // Connect to default postgres database
        };
        return adminBuilder.ConnectionString;
    }

    private static string? GetDatabaseNameToCreate(string connectionString)
    {
        NpgsqlConnectionStringBuilder builder = new(connectionString);
        string? targetDatabase = builder.Database;
    
        // If no database specified, just return because there is no database to ensure that it is created
        if (string.IsNullOrEmpty(targetDatabase))
        {
            return null;
        }

        return targetDatabase;
    }
}