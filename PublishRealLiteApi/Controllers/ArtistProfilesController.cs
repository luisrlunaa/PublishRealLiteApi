using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistProfilesController : ApiControllerBase
    {
        private readonly IArtistProfileService _service;
        public ArtistProfilesController(IArtistProfileService service, ICurrentUserService currentUser) : base(currentUser)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [Authorize]
        [HttpGet("me/admin")]
        public async Task<IActionResult> GetMyAdminProfile()
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();

            var profile = await _service.GetAdminProfileWithSubProfilesAsync(userId);
            if (profile == null) return NotFound("No admin profile found");

            return Ok(profile);
        }

        [Authorize]
        [HttpGet("me/subprofiles")]
        public async Task<IActionResult> GetMySubProfiles()
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();

            var profiles = await _service.GetSubProfilesAsync(userId);
            return Ok(profiles);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArtistDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var result = await _service.CreateAsync(userId, dto);
            if (result == null) return BadRequest("The user already has a profile.");
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [Authorize]
        [HttpPost("with-admin-code")]
        public async Task<IActionResult> CreateWithAdminCode([FromBody] CreateArtistWithAdminCodeDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();

            var profile = await _service.CreateWithAdminCodeAsync(userId, dto);
            if (profile == null)
                return BadRequest("Invalid admin code or profile already exists");

            return CreatedAtAction(nameof(Get), new { id = profile.Id }, profile);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArtistDto dto)
        {
            var userId = CurrentUser.UserId;
            var isAdmin = CurrentUser.IsAdmin;
            var ok = await _service.UpdateAsync(id, userId!, isAdmin, dto);
            if (!ok) return Forbid();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUser.UserId;
            var isAdmin = CurrentUser.IsAdmin;
            var ok = await _service.DeleteAsync(id, userId!, isAdmin);
            if (!ok) return Forbid();
            return NoContent();
        }
    }
}