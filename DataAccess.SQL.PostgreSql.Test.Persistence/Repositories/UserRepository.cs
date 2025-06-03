using DataAccess.SQL.PostgreSql.Test.Application.Models;
using DataAccess.SQL.PostgreSql.Test.Application.Repositories.Abstractions;

namespace DataAccess.SQL.PostgreSql.Test.Persistence.Repositories;

public class UserRepository(PostgreSqlDbContext context) : IUserRepository
{
    private const string AddUserSql = @"INSERT INTO users(name,  last_name, age) VALUES (@Name, @LastName, @Age)";

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await context.ExecuteAsync(AddUserSql, user, cancellationToken);
    }
}