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