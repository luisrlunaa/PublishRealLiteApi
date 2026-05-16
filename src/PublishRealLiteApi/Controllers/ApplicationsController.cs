using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Features.Applications;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitApplication.Command cmd,
        [FromServices] SubmitApplication.Handler handler)
    {
        var result = await handler.HandleAsync(cmd);
        if (result == null) return BadRequest(new { message = "Something went wrong, please try again." });
        return Ok();
    }
}
