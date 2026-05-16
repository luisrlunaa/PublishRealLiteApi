using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.Teams;

namespace PublishRealLiteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController(ICurrentUserService currentUser) : ApiControllerBase(currentUser)
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromServices] GetMyTeams.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var result = await handler.HandleAsync(new GetMyTeams.Query());
        if (result == null) return NotFound("No profile found");
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeam.Command cmd,
        [FromServices] CreateTeam.Handler handler)
    {
        if (CurrentUser.UserId == null) return Unauthorized();
        var result = await handler.HandleAsync(cmd);
        if (result == null) return BadRequest("No profile found");
        return CreatedAtAction(nameof(Mine), new { id = result.Id }, result);
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite(
        [FromBody] InviteTeamMember.Command cmd,
        [FromServices] InviteTeamMember.Handler handler)
    {
        await handler.HandleAsync(cmd);
        return Ok();
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] string token,
        [FromServices] AcceptTeamInvite.Handler handler)
    {
        var email = CurrentUser.Email ?? throw new InvalidOperationException("No email");
        await handler.HandleAsync(new AcceptTeamInvite.Command(token, email));
        return Ok();
    }
}
