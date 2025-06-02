namespace DataAccess.SQL.ManualTesting.Repositories.Abstractions;

public interface IUsersRepository
{
    Task<string> GetSingleUserNameAsync();
}