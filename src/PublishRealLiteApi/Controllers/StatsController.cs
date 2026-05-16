using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Features.Stats;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(
        [FromServices] GetStatsSummary.Handler handler,
        [FromQuery] int range = 30)
    {
        var result = await handler.HandleAsync(new GetStatsSummary.Query(range));
        return Ok(result);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(
        [FromBody] List<ImportStats.StreamStatDto> items,
        [FromServices] ImportStats.Handler handler)
    {
        if (items == null || items.Count == 0) return BadRequest("No data");
        var result = await handler.HandleAsync(new ImportStats.Command(items));
        return Ok(new { imported = result.Imported });
    }
}
