using Microsoft.AspNetCore.Mvc;

namespace DataAccess.SQL.ManualTesting.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    [HttpPost("{data}")]
    public async Task<IActionResult> Test(string data)
    {
        return Ok();
    }
}