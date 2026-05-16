using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Features.Auth;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] Register.Command cmd,
        [FromServices] Register.Handler handler)
    {
        try
        {
            var result = await handler.HandleAsync(cmd);
            if (result == null) return BadRequest(new { message = "Something went wrong, please try again." });
            return Ok(new { result.Id, result.Email });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] Login.Command cmd,
        [FromServices] Login.Handler handler)
    {
        var result = await handler.HandleAsync(cmd);
        if (result == null) return Unauthorized("Invalid credentials");
        return Ok(result);
    }
}
