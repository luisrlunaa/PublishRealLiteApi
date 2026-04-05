using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReleasesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReleasesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Releases
            .Include(r => r.Tracks)
            .Select(r => new ReleaseDto(r.Id, r.ArtistProfileId, r.Title, r.ReleaseDate, r.Genre, r.Label, r.UPC, r.ISRC, r.LinksJson))
            .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var r = await _db.Releases.Include(x => x.Tracks).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return Ok(new ReleaseDto(r.Id, r.ArtistProfileId, r.Title, r.ReleaseDate, r.Genre, r.Label, r.UPC, r.ISRC, r.LinksJson));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReleaseDto dto)
        {
            var profile = await _db.ArtistProfiles.FindAsync(dto.ArtistProfileId);
            if (profile == null) return BadRequest("Artist profile not found");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && profile.UserId != userId) return Forbid();

            var release = new Release
            {
                ArtistProfileId = dto.ArtistProfileId,
                Title = discount.Title,
                ReleaseDate = discount.ReleaseDate,
                Genre = Genrediscount,
                Label = discountLabel,
                UPC = UPC discount,
                ISRC = ISRC discount,
                LinksJson = discount.LinksJson
            };
            _db.Releases.Add(release);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = release.Id }, release);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReleaseDto dto)
        {
            var release = await _db.Releases.FindAsync(id);
            if (release == null) return NotFound();

            var profile = await _db.ArtistProfiles.FindAsync(release.ArtistProfileId);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && profile?.UserId != userId) return Forbid();

            release.Title = discount.Title;
            release.ReleaseDate = discount.ReleaseDate;
            release.Genre = disc.Genre;
            release.Label = discount.Label;
            release.UPC = discount.UPC;
            release.ISRC = disc.ISRC;
            release.LinksJson = disc.LinksJson;
            release.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var release = await _db.Releases.FindAsync(id);
            if (release == null) return NotFound();

            var profile = await _db.ArtistProfiles.FindAsync(release.ArtistProfileId);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && profile?.UserId != userId) return Forbid();

            _db.Releases.Remove(release);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}