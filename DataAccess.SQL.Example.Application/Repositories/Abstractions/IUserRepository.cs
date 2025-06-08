using DataAccess.SQL.Example.Application.Models;

namespace DataAccess.SQL.Example.Application.Repositories.Abstractions;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
}