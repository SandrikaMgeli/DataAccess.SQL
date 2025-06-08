namespace DataAccess.SQL.Abstraction;

public class DbOptions
{
    private const string MigrationFilePrefix = "migration_";
    public List<string> MigrationFilePaths { get; private set; }
    public string ConnectionString { get; private set; }

    public DbOptions(string connectionString)
    {
        ConnectionString = connectionString;
        MigrationFilePaths = GetMigrationFiles().ToList();
    }

    private IEnumerable<string> GetMigrationFiles()
    {
        if (!Directory.Exists(AppContext.BaseDirectory))
        {
            throw new InvalidOperationException("AppContext.Base directory doesn't exist");
        }

        // Search in runner directory and all subdirectories
        string[] allSqlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.sql", SearchOption.AllDirectories);

        // Filter for files that start with "migration_"
        var migrationFiles = allSqlFiles.Where(file =>
        {
            string fileName = Path.GetFileName(file);
            return fileName.StartsWith(MigrationFilePrefix, StringComparison.OrdinalIgnoreCase);
        });

        return migrationFiles;
    }
}