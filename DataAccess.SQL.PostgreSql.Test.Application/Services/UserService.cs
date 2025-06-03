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