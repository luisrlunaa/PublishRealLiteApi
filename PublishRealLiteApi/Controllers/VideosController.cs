using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ApiControllerBase
    {
        private readonly IArtistVideoService _service;
        private readonly IAuthService _auth;
        public VideosController(IArtistVideoService service, IAuthService auth, ICurrentUserService currentUser) : base(currentUser)
        {
            _service = service;
            _auth = auth;
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            // Use AuthService to resolve current user's profile id
            try
            {
                var profileId = await _auth.GetProfileIdAsync();
                var videos = await _service.GetByArtistProfileIdAsync(profileId);
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return BadRequest("No artist profile found for current user");
            }
        }

        [HttpGet("profile/{profileId:int}")]
        public async Task<IActionResult> GetByProfile(int profileId)
        {
            var videos = await _service.GetByArtistProfileIdAsync(profileId);
            return Ok(videos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArtistVideoDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            var video = await _service.CreateAsync(dto, userId!, isAdmin);
            return CreatedAtAction(nameof(GetMine), new { id = video.Id }, video);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArtistVideoDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            var ok = await _service.UpdateAsync(id, dto, userId!, isAdmin);
            if (!ok) return Forbid();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int artistProfileId)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            var ok = await _service.DeleteAsync(id, userId!, isAdmin, artistProfileId);
            if (!ok) return Forbid();
            return NoContent();
        }
    }
}
