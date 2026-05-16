using Microsoft.AspNetCore.Mvc;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly PublishRealLiteApi.Services.Interfaces.IStorageService _storage;
        public UploadsController(PublishRealLiteApi.Services.Interfaces.IStorageService storage) => _storage = storage;

        [HttpPost("image")]
        [RequestSizeLimit(4 * 1024 * 1024)] // 4MB limit at Kestrel level
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "covers")
        {
            if (file == null) return BadRequest("No file");
            try
            {
                var res = await _storage.SaveImageAsync(file, folder);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}
