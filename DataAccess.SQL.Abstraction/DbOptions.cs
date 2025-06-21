using System.Text.RegularExpressions;

namespace DataAccess.SQL.Abstraction;

public class DbOptions
{
    private const string MigrationFilePrefix = "migration_";

    /// <summary>
    /// Matches strings that start with "migration_", followed by a number,
    /// then an underscore, then any other characters.
    /// </summary>
    private const string RegexForValidMigrations = @"^migration_\d+_.*$";

    /// <summary>
    /// Ordered from oldest to newest
    /// </summary>
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
        IEnumerable<string> migrationFiles = allSqlFiles
            .Where(file =>
            {
                string fileName = Path.GetFileName(file);
                return fileName.StartsWith(MigrationFilePrefix, StringComparison.OrdinalIgnoreCase) &&
                       Regex.IsMatch(fileName, RegexForValidMigrations);
            })
            .OrderBy(file =>
            {
                string[] splitedFileName = Path.GetFileName(file).Split('_');
                int indexIdentifier = Convert.ToInt32(splitedFileName[1]);
                return indexIdentifier;
            });

        return migrationFiles;
    }
}