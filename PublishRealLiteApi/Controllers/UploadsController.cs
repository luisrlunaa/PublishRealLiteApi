using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadService _upload;
        public UploadsController(IUploadService upload) => _upload = upload;

        [HttpPost("image")]
        [RequestSizeLimit(4 * 1024 * 1024)] // 4MB limit at Kestrel level
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "covers")
        {
            if (file == null) return BadRequest("No file");
            try
            {
                var res = await _upload.SaveImageAsync(file, folder);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}
