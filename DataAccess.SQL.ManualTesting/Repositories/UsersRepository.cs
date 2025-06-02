using DataAccess.SQL.ManualTesting.Repositories.Abstractions;
using DataAccess.SQL.PostgreSql;

namespace DataAccess.SQL.ManualTesting.Repositories;

public class UsersRepository(PostgreSqlDbContext context) : IUsersRepository
{
    public Task<string> GetSingleUserNameAsync()
    {
        throw new NotImplementedException();
    }
}