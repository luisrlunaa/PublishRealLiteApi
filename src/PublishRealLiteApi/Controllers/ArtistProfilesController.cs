using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.ArtistProfiles;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistProfilesController(ICurrentUserService currentUser) : ApiControllerBase(currentUser)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] GetArtistProfiles.Handler handler)
    {
        var result = await handler.HandleAsync(new GetArtistProfiles.Query());
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, [FromServices] GetArtistProfileById.Handler handler)
    {
        var result = await handler.HandleAsync(new GetArtistProfileById.Query(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me/admin")]
    public async Task<IActionResult> GetMyAdminProfile([FromServices] GetMyAdminProfile.Handler handler)
    {
        var userId = CurrentUser.UserId;
        if (userId == null) return Unauthorized();
        var result = await handler.HandleAsync(new GetMyAdminProfile.Query());
        if (result == null) return NotFound("No admin profile found");
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me/subprofiles")]
    public async Task<IActionResult> GetMySubProfiles([FromServices] GetMySubProfiles.Handler handler)
    {
        var result = await handler.HandleAsync(new GetMySubProfiles.Query());
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateArtistProfile.Command cmd,
        [FromServices] CreateArtistProfile.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var result = await handler.HandleAsync(cmd);
        if (result == null) return BadRequest("The user already has a profile.");
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("with-admin-code")]
    public async Task<IActionResult> CreateWithAdminCode(
        [FromBody] CreateArtistProfileWithAdminCode.Command cmd,
        [FromServices] CreateArtistProfileWithAdminCode.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var result = await handler.HandleAsync(cmd);
        if (result == null) return BadRequest("Invalid admin code or profile already exists");
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateArtistProfile.Command cmd,
        [FromServices] UpdateArtistProfile.Handler handler)
    {
        var ok = await handler.HandleAsync(cmd with { Id = id });
        if (!ok) return Forbid();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteArtistProfile.Handler handler)
    {
        var ok = await handler.HandleAsync(new DeleteArtistProfile.Command(id));
        if (!ok) return Forbid();
        return NoContent();
    }
}
