using DataAccess.SQL.Example.Application.Models;

namespace DataAccess.SQL.Example.Application.Services.Abstractions;

public interface IUserService
{
    Task AddUsers(List<User> user, CancellationToken cancellationToken);   
}