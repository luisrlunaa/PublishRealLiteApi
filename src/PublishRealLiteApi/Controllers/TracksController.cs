using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.Tracks;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(ICurrentUserService currentUser) : ApiControllerBase(currentUser)
{
    [HttpGet("by-release/{releaseId:guid}")]
    public async Task<IActionResult> GetByRelease(
        Guid releaseId,
        [FromServices] GetTracksByRelease.Handler handler)
    {
        var result = await handler.HandleAsync(new GetTracksByRelease.Query(releaseId));
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTrack.Command cmd,
        [FromServices] CreateTrack.Handler handler)
    {
        var result = await handler.HandleAsync(cmd);
        if (result == null) return Forbid();
        return CreatedAtAction(nameof(GetByRelease), new { releaseId = result.ReleaseId }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTrack.Command cmd,
        [FromServices] UpdateTrack.Handler handler)
    {
        var ok = await handler.HandleAsync(cmd with { Id = id });
        if (!ok) return Forbid();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] DeleteTrack.Handler handler)
    {
        var ok = await handler.HandleAsync(new DeleteTrack.Command(id));
        if (!ok) return Forbid();
        return NoContent();
    }
}
