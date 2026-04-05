using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.Services.Interfaces;
using System.Security.Claims;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IUploadService _upload;
        public VideosController(AppDbContext db, IUploadService upload) { _db = db; _upload = upload; }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _db.ArtistProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();
            var videos = await _db.ArtistVideos.Where(v => v.ArtistProfileId == profile.Id).ToListAsync();
            return Ok(videos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArtistVideoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _db.ArtistProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            var v = new ArtistVideo
            {
                ArtistProfileId = profile.Id,
                Title = dto.Title,
                ThumbnailUrl = dto.ThumbnailUrl,
                VideoUrl = dto.VideoUrl
            };
            _db.ArtistVideos.Add(v);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMine), new { id = v.Id }, v);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArtistVideoDto dto)
        {
            var v = await _db.ArtistVideos.FindAsync(id);
            if (v == null) return NotFound();
            v.Title = dto.Title;
            v.ThumbnailUrl = dto.ThumbnailUrl;
            v.VideoUrl = dto.VideoUrl;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _db.ArtistVideos.FindAsync(id);
            if (v == null) return NotFound();
            _db.ArtistVideos.Remove(v);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

}
