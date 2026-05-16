using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.Videos;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController(ICurrentUserService currentUser) : ApiControllerBase(currentUser)
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromServices] GetMyVideos.Handler handler)
    {
        var result = await handler.HandleAsync(new GetMyVideos.Query());
        if (result == null) return BadRequest("No artist profile found for current user");
        return Ok(result);
    }

    [HttpGet("profile/{profileId:int}")]
    public async Task<IActionResult> GetByProfile(
        int profileId,
        [FromServices] GetVideosByProfile.Handler handler)
    {
        var result = await handler.HandleAsync(new GetVideosByProfile.Query(profileId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVideo.Command cmd,
        [FromServices] CreateVideo.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var result = await handler.HandleAsync(cmd);
        if (result == null) return Forbid();
        return CreatedAtAction(nameof(GetMine), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVideo.Command cmd,
        [FromServices] UpdateVideo.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var ok = await handler.HandleAsync(cmd with { Id = id });
        if (!ok) return Forbid();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] int artistProfileId,
        [FromServices] DeleteVideo.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var ok = await handler.HandleAsync(new DeleteVideo.Command(id, artistProfileId));
        if (!ok) return Forbid();
        return NoContent();
    }
}
