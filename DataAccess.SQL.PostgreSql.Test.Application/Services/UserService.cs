using System.Collections.Concurrent;
using DataAccess.SQL.PostgreSql.Test.Application.Models;
using DataAccess.SQL.PostgreSql.Test.Application.Repositories.Abstractions;
using DataAccess.SQL.PostgreSql.Test.Application.Services.Abstractions;

namespace DataAccess.SQL.PostgreSql.Test.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task AddUsers(List<User> users, CancellationToken cancellationToken)
    {
        // if you are not using transaction, then you can use connection pool to run parallel queries like that
        /* Parallel.ForEach implementation is designed in way that even if there were multiple exceptions happened
         it only throws the first one. so even if all the inserts fails, it throws only single exception (The first one).
         so it is better practise to handle that scenarios by hand. this is the reason why we have written try/catch block
         in the body of ForEachAsync*/

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