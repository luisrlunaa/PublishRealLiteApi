using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.Releases;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReleasesController(ICurrentUserService currentUser) : ApiControllerBase(currentUser)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int artistProfileId,
        [FromServices] GetReleases.Handler handler)
    {
        var result = await handler.HandleAsync(new GetReleases.Query(artistProfileId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] GetReleaseById.Handler handler)
    {
        var result = await handler.HandleAsync(new GetReleaseById.Query(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRelease.Command cmd,
        [FromServices] CreateRelease.Handler handler)
    {
        var result = await handler.HandleAsync(cmd);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRelease.Command cmd,
        [FromServices] UpdateRelease.Handler handler)
    {
        var ok = await handler.HandleAsync(cmd with { Id = id });
        if (!ok) return Forbid();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] int artistProfileId,
        [FromServices] DeleteRelease.Handler handler)
    {
        var ok = await handler.HandleAsync(new DeleteRelease.Command(id, artistProfileId));
        if (!ok) return Forbid();
        return NoContent();
    }
}
