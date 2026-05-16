using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ApiControllerBase
    {
        private readonly AppDbContext _db;
        public TracksController(AppDbContext db, ICurrentUserService currentUser) : base(currentUser) => _db = db;

        [HttpGet("by-release/{releaseId:guid}")]
        public async Task<IActionResult> GetByRelease(Guid releaseId)
        {
            var tracks = await _db.Tracks.Where(t => t.ReleaseId == releaseId).OrderBy(t => t.Position)
            .Select(t => new TrackDto(t.Id, t.ReleaseId, t.Position, t.Title)).ToListAsync();
            return Ok(tracks);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTrackDto dto)
        {
            var release = await _db.Releases.FindAsync(dto.ReleaseId);
            if (release == null) return BadRequest("Release not found");

            var profile = await _db.ArtistProfiles.FindAsync(release.ArtistProfileId);
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            if (!isAdmin && profile?.UserId != userId) return Forbid();

            var track = new Track { ReleaseId = dto.ReleaseId, Position = dto.Position, Title = dto.Title };
            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByRelease), new { releaseId = dto.ReleaseId }, track);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrackDto dto)
        {
            var track = await _db.Tracks.FindAsync(id);
            if (track == null) return NotFound();

            var release = await _db.Releases.FindAsync(track.ReleaseId);
            var profile = await _db.ArtistProfiles.FindAsync(release?.ArtistProfileId);
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            if (!isAdmin && profile?.UserId != userId) return Forbid();

            track.Position = track.Position;
            track.Title = track.Title;
            track.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var track = await _db.Tracks.FindAsync(id);
            if (track == null) return NotFound();

            var release = await _db.Releases.FindAsync(track.ReleaseId);
            var profile = await _db.ArtistProfiles.FindAsync(release?.ArtistProfileId);
            var userId = CurrentUser.UserId;
            var isAdmin = CurrentUser.IsAdmin;
            if (!isAdmin && profile?.UserId != userId) return Forbid();

            _db.Tracks.Remove(track);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}