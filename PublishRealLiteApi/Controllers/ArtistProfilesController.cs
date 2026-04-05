using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistProfilesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ArtistProfilesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.ArtistProfiles
            .Select(p => new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson))
            .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _db.ArtistProfiles.FindAsync(id);
            if (p == null) return NotFound();
            return Ok(new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArtistDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Avoid duplicates per user 
            if (await _db.ArtistProfiles.AnyAsync(x => x.UserId == userId))
                return BadRequest("The user already has a profile.");

            var profile = new ArtistProfile
            {
                UserId = userId,
                ArtistName = dto.ArtistName,
                Bio = dto.Bio,
                SocialLinksJson = dto.SocialLinksJson
            };

            _db.ArtistProfiles.Add(profile);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = profile.Id }, profile);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArtistDto dto)
        {
            var profile = await _db.ArtistProfiles.FindAsync(id);
            if (profile == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && profile.UserId != userId) return Forbid();

            profile.ArtistName = dto.ArtistName;
            profile.Bio = dto.Bio;
            profile.SocialLinksJson = dto.SocialLinksJson;
            profile.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _db.ArtistProfiles.FindAsync(id);
            if (profile == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && profile.UserId != userId) return Forbid();

            _db.ArtistProfiles.Remove(profile);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}