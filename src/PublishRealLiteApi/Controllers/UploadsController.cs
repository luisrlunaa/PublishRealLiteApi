using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Features.Uploads;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    [HttpPost("image")]
    [RequestSizeLimit(4 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromQuery] string folder = "covers",
        [FromServices] UploadImage.Handler handler)
    {
        if (file == null) return BadRequest("No file");
        try
        {
            var result = await handler.HandleAsync(new UploadImage.Command(file, folder));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
