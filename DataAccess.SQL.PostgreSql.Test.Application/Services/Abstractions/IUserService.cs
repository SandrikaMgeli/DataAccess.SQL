using DataAccess.SQL.PostgreSql.Test.Application.Models;

namespace DataAccess.SQL.PostgreSql.Test.Application.Services.Abstractions;

public interface IUserService
{
    Task AddUsers(List<User> user, CancellationToken cancellationToken);   
}