using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ApiControllerBase
    {
        private readonly ITeamService _teams;
        public TeamsController(ITeamService teams, ICurrentUserService currentUser) : base(currentUser) => _teams = teams;

        [HttpGet("mine")]
        public async Task<IActionResult> Mine()
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var profileId = await GetProfileIdForUser(userId);
            var teams = await _teams.GetTeamsForArtistAsync(profileId);
            return Ok(teams);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest req)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var profileId = await GetProfileIdForUser(userId);
            var team = await _teams.CreateTeamAsync(profileId, req.Name);
            return CreatedAtAction(nameof(Mine), new { id = team.Id }, team);
        }

        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] InviteRequest req)
        {
            await _teams.InviteAsync(req.TeamId, req.Email);
            return Ok();
        }

        [HttpPost("accept")]
        public async Task<IActionResult> Accept([FromQuery] string token)
        {
            var email = CurrentUser.Email ?? throw new InvalidOperationException("No email");
            await _teams.AcceptInviteAsync(token, email);
            return Ok();
        }

        private async Task<int> GetProfileIdForUser(string userId)
        {
            // implement lookup
            using var scope = HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var profile = await db.ArtistProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) throw new InvalidOperationException("No profile");
            return profile.Id;
        }
    }

    public class CreateTeamRequest { public string Name { get; set; } = string.Empty; }
    public class InviteRequest { public int TeamId { get; set; } public string Email { get; set; } = string.Empty; }

}
