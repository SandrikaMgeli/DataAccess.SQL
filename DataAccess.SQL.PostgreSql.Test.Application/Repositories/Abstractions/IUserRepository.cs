using DataAccess.SQL.PostgreSql.Test.Application.Models;

namespace DataAccess.SQL.PostgreSql.Test.Application.Repositories.Abstractions;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
}