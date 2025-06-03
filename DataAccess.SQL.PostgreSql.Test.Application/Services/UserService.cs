using DataAccess.SQL.PostgreSql.Test.Application.Models;
using DataAccess.SQL.PostgreSql.Test.Application.Repositories.Abstractions;
using DataAccess.SQL.PostgreSql.Test.Application.Services.Abstractions;

namespace DataAccess.SQL.PostgreSql.Test.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task AddUsers(List<User> users, CancellationToken cancellationToken)
    {
        // if you are not using transaction, then you can use connection pool to run parallel queries like that

        await Parallel.ForEachAsync(
            users, 
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = cancellationToken,
            },
            async (user , token) =>
            {
                await userRepository.AddUserAsync(user, token);
            });
    }
}