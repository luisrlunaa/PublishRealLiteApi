using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.DTOs;
using PublishRealLiteApi.Application.Services.Interfaces;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IArtistApplicationService _applicationService;

        public ApplicationsController(IArtistApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitApplicationDto dto)
        {
            var success = await _applicationService.SubmitAsync(dto);
            if (!success)
                return BadRequest(new { message = "Something went wrong, please try again." });

            return Ok();
        }
    }
}
