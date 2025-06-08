using DataAccess.SQL.Example.Application.Models;
using DataAccess.SQL.Example.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace DataAccess.SQL.PostgreSql.Example.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddUsers([FromBody] List<User> users, CancellationToken cancellationToken)
    {
        await userService.AddUsers(users, cancellationToken);
        return Ok();
    }
}