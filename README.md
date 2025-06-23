# DataAccess.SQL

A lightweight, high-performance SQL data access library for .NET 9 with PostgreSQL support, featuring connection pooling, transaction management, and parallel query execution capabilities.

## Features

- **Connection Pooling**: Automatic connection pool management for optimal performance
- **Parallel Query Execution**: Execute multiple queries concurrently when not in transaction mode
- **Transaction Support**: Full ACID transaction support with isolation level control
- **Migration System**: Automatic database migration with distributed locking
- **Repository Pattern**: Clean abstraction for data access operations
- **Dependency Injection**: First-class DI support
- **Async/Await**: Full async support throughout the library

## Installation

```bash
# Install the abstraction package
dotnet add package SandrikaMgeli.DataAccess.SQL.Abstraction

# Install PostgreSQL implementation
dotnet add package SandrikaMgeli.DataAccess.SQL.PostgreSql
```
IDbManager is added as Scoped.
IDbContext is added as Scoped.
DbOptions is added as Singleton.

## Configuration

### Dependency Injection Setup

```csharp
using DataAccess.SQL.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Register DataAccess.SQL services
builder.Services.AddPostgreSql(
    builder.Configuration.GetConnectionString("PostgreSql")!
);

// Register your repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Setup database (runs migrations, ensures DB exists)
app.SetupPostgreSql();

app.Run();
```

### Connection String Configuration

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Pooling=True;Maximum Pool Size=50;Minimum Pool Size=10;Connection Idle Lifetime=60;Host=localhost;Port=5432;Database=myapp_db;Username=myuser;Password=mypassword"
  }
}
```

## Architecture & Internal Workings

### Core Components

1. **DbContext**: Abstract base class providing database operations
2. **IDbManager**: Interface for transaction management
3. **DbOptions**: Configuration and migration file discovery
4. **PostgreSqlDbContext**: PostgreSQL-specific implementation

### State Management

The library operates in two modes:

- **None**: Uses connection pooling, allows  execution
- **Transactional**: Uses single connection with transaction, sequential execution

```csharp
public enum DbManagerState
{
    None = 0,        // Connection pooling mode
    Transactional = 1 // Transaction mode
}
```

### Connection Pool Strategy

When not in transaction mode, each query gets a fresh connection from the pool:

```csharp
private async Task<T> RunAsync<T>(Func<NpgsqlConnection, Task<T>> target)
{
    if (_state == DbManagerState.None)
    {
        // Gets connection from pool, executes query, returns connection
        await using NpgsqlConnection connection = GetConnectionFromPool();
        return await target(connection);
    }
    // ... transaction handling
}
```

## Repository Implementation

### 1. Define Your Models

```csharp
namespace MyApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. Create Repository Interface

```csharp
using MyApp.Models;

namespace MyApp.Repositories.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### 3. Implement Repository

```csharp
using DataAccess.SQL.Abstraction;
using MyApp.Models;
using MyApp.Repositories.Abstractions;

namespace MyApp.Repositories;

public class UserRepository(DbContext context) : IUserRepository
{
    private const string GetByIdSql = "SELECT * FROM users WHERE id = @Id";
    private const string GetAllSql = "SELECT * FROM users ORDER BY created_at DESC";
    private const string InsertSql = @"
        INSERT INTO users (name, last_name, age, created_at) 
        VALUES (@Name, @LastName, @Age, @CreatedAt) 
        RETURNING id";
    private const string UpdateSql = @"
        UPDATE users 
        SET name = @Name, last_name = @LastName, age = @Age 
        WHERE id = @Id";
    private const string DeleteSql = "DELETE FROM users WHERE id = @Id";

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.QuerySingleOrDefaultAsync<User>(GetByIdSql, new { Id = id }, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.QueryAsync<User>(GetAllSql, cancellationToken: cancellationToken);
    }

    public async Task<int> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        return await context.QuerySingleAsync<int>(InsertSql, user, cancellationToken);
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        int rowsAffected = await context.ExecuteAsync(UpdateSql, user, cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        int rowsAffected = await context.ExecuteAsync(DeleteSql, new { Id = id }, cancellationToken);
        return rowsAffected > 0;
    }
}
```

##  Query Execution

### When NOT Using Transactions

The library automatically uses connection pooling, allowing safe parallel execution:

```csharp
using System.Collections.Concurrent;
using DataAccess.SQL.Example.Application.Models;
using DataAccess.SQL.Example.Application.Repositories.Abstractions;
using DataAccess.SQL.Example.Application.Services.Abstractions;

namespace DataAccess.SQL.Example.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task AddUsers(List<User> users, CancellationToken cancellationToken)
    {
        // if you are not using transaction, then you can use connection pool to run parallel queries like that
        /* The Parallel.ForEach implementation is designed in a way that even if multiple exceptions occur, 
           it only throws the first one. So even if all iterations fail, it throws only a single exception 
           (the first one). Therefore, it is better practice to handle such scenarios manually. This is the 
           reason why we have written a try/catch block in the body of ForEach.*/

        ConcurrentBag<Exception> happenedExceptions = [];

        await Parallel.ForEachAsync(
            users, 
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = cancellationToken,
            },
            async (user , token) =>
            {
                try
                {
                    await userRepository.AddUserAsync(user, token);
                }
                catch (Exception ex)
                {
                    happenedExceptions.Add(ex);
                }
            });

        // Logging all the happened exceptions here
        foreach (Exception happenedException in happenedExceptions)
        {
            Console.WriteLine(happenedException.Message);
        }
    }
}
```

### Parallel Bulk Operations Example

```csharp
public class BulkOperationService(IUserRepository userRepository, IOrderRepository orderRepository)
{
    public async Task ProcessBulkDataAsync(
        List<User> users, 
        List<Order> orders, 
        CancellationToken cancellationToken)
    {
        // Execute different operations in parallel
        var userTask = ProcessUsersAsync(users, cancellationToken);
        var orderTask = ProcessOrdersAsync(orders, cancellationToken);

        // Wait for all operations to complete
        await Task.WhenAll(userTask, orderTask);
    }

    private async Task ProcessUsersAsync(List<User> users, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(users, 
            new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
            async (user, token) => await userRepository.AddAsync(user, token));
    }

    private async Task ProcessOrdersAsync(List<Order> orders, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(orders,
            new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cancellationToken },
            async (order, token) => await orderRepository.AddAsync(order, token));
    }
}
```

## Transaction Management

### Using Transactions with IDbManager

```csharp
public class TransactionalUserService(IDbManager dbManager, IUserRepository userRepository) : IUserService
{
    public async Task CreateUserWithProfileAsync(User user, UserProfile profile, CancellationToken cancellationToken)
    {
        // Start a transaction
        await using var transactionalContext = await dbManager.RunWithTransactionAsync(
            IsolationLevel.ReadCommitted, 
            cancellationToken);

        try
        {
            // All operations within this scope use the same connection & transaction
            var userId = await userRepository.AddAsync(user, cancellationToken);
            profile.UserId = userId;
            await profileRepository.AddAsync(profile, cancellationToken);

            await transactionalContext.CommitAsync();
        }
        catch
        {
            // Transaction is rolled back automatically on exception, but still you can write await transactionalContext.RollbackAsync() if you need to explicitly call it.
            throw;
        }
    }
}
```

### Synchronous Transaction Example

```csharp
public class UserService(IDbManager dbManager, IUserRepository userRepository) : IUserService
{
    public void CreateUserWithProfile(User user, UserProfile profile)
    {
        using var transactionalContext = dbManager.RunWithTransaction(IsolationLevel.ReadCommitted);
        
        try
        {
            var userId = userRepository.Add(user);
            profile.UserId = userId;
            profileRepository.Add(profile);
            
            transactionalContext.Commit();
        }
        catch
        {
            // Rolls back automatically on exception
            throw;
        }
    }
}
```

### Different Isolation Levels

```csharp
// Read Uncommitted - lowest isolation, highest performance
await using var context1 = await dbManager.RunWithTransactionAsync(IsolationLevel.ReadUncommitted);

// Read Committed - default level, prevents dirty reads
await using var context2 = await dbManager.RunWithTransactionAsync(IsolationLevel.ReadCommitted);

// Repeatable Read - prevents dirty and non-repeatable reads
await using var context3 = await dbManager.RunWithTransactionAsync(IsolationLevel.RepeatableRead);

// Serializable - highest isolation, prevents all phenomena
await using var context4 = await dbManager.RunWithTransactionAsync(IsolationLevel.Serializable);
```

## Database Migrations

### Migration File Naming Convention

Create SQL files with the naming pattern: `migration_{number}_{description}.sql`
This numbers are needed to be executed in correct order. if number is less, this migration is executed before bigger numbers.

```
Migrations/
├── migration_1_create_users_table.sql
├── migration_2_add_user_profiles.sql
├── migration_3_add_indexes.sql
└── migration_4_alter_user_constraints.sql
```

### Example Migration File

**migration_1_create_users_table.sql:**
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    age INTEGER NOT NULL CHECK (age >= 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_name ON users(name);
CREATE INDEX idx_users_created_at ON users(created_at);
```

### Migration Features

- **Automatic Discovery**: Migrations are discovered automatically from the application directory
- **Distributed Locking**: Uses PostgreSQL advisory locks to prevent concurrent migrations
- **Order Enforcement**: Migrations run in numerical order
- **Idempotent**: Each migration runs only once, tracked in `applied_migrations` table

## Controller Examples

### Basic CRUD Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user, CancellationToken cancellationToken)
    {
        var userId = await userService.AddAsync(user, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = userId }, user);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateUsersBulk([FromBody] List<User> users, CancellationToken cancellationToken)
    {
        await userService.ProcessUsersInParallelAsync(users, cancellationToken);
        return Ok(new { message = $"Processed {users.Count} users successfully" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user, CancellationToken cancellationToken)
    {
        user.Id = id;
        var updated = await userService.UpdateAsync(user, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var deleted = await userService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
```

## Available Query Methods

### Synchronous Methods
```csharp
// Single record operations
T QuerySingle<T>(string sql, object? params = null);
T? QuerySingleOrDefault<T>(string sql, object? params = null);
T QueryFirst<T>(string sql, object? params = null);
T? QueryFirstOrDefault<T>(string sql, object? params = null);

// Multiple records
IEnumerable<T> Query<T>(string sql, object? params = null);

// Execute commands
int Execute(string sql, object? params = null);
```

### Asynchronous Methods
```csharp
// Single record operations
Task<T> QuerySingleAsync<T>(string sql, object? params = null, CancellationToken cancellationToken = default);
Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? params = null, CancellationToken cancellationToken = default);
Task<T> QueryFirstAsync<T>(string sql, object? params = null, CancellationToken cancellationToken = default);
Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? params = null, CancellationToken cancellationToken = default);

// Multiple records
Task<IEnumerable<T>> QueryAsync<T>(string sql, object? params = null, CancellationToken cancellationToken = default);

// Execute commands
Task<int> ExecuteAsync(string sql, object? params = null, CancellationToken cancellationToken = default);
```

## Important Notes

### Performance Considerations

1. **Connection Pooling**: The library uses connection pooling effectively. Configure your connection string pool settings based on your load:
   ```
   Maximum Pool Size=50;Minimum Pool Size=10;Connection Idle Lifetime=60
   ```

2. **Parallel Execution**: Only use parallel execution when NOT in transaction mode. Transactions require sequential execution on a single connection.

3. **Resource Management**: Always use `using` statements or proper disposal patterns with transactions.

### Best Practices

1. **Repository Pattern**: Keep SQL queries in repositories, not in services
2. **Parameterized Queries**: Always use parameterized queries to prevent SQL injection
3. **Async All The Way**: Use async methods consistently throughout your application
4. **Transaction Scope**: Keep transaction scopes as small as possible
5. **Error Handling**: Implement proper exception handling, especially in parallel operations

### Thread Safety

- **DbContext**: Each request gets its own scoped DbContext instance
- **Connection Pooling**: Thread-safe connection pool management
- **Transactions**: Not thread-safe - use single-threaded execution within transaction scope

## Contributing

This library is experimental and designed for learning purposes. While functional, it's recommended to use established ORMs like Entity Framework Core or Dapper directly for production applications.
It's stable version DataAccess.SQL.Events library is used in GameHub project;
