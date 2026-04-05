using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _stats;
        public StatsController(IStatsService stats) => _stats = stats;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] int range = 30)
        {
            var summary = await _stats.GetSummaryAsync(range);
            return Ok(summary);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] IEnumerable<StreamStatDto> items)
        {
            if (items == null || !items.Any()) return BadRequest("No data");
            await _stats.ImportAsync(items);
            return Ok(new { imported = items.Count() });
        }
    }

}
