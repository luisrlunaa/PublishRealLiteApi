using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReleasesController : ApiControllerBase
    {
        private readonly IReleaseService _service;
        public ReleasesController(IReleaseService service, ICurrentUserService currentUser) : base(currentUser) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int artistProfileId)
        {
            var list = Enumerable.Empty<ReleaseDto>();
            var userId = CurrentUser.UserId;
            var isAdmin = CurrentUser.IsAdmin;
            if (isAdmin)
                list = await _service.GetAllAsync(userId);
            else
                list = await _service.GetAllAsync(artistProfileId);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var r = await _service.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReleaseDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            var result = await _service.CreateAsync(userId!, isAdmin, dto);
            if (result == null) return Forbid();
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReleaseDto dto)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var isAdmin = CurrentUser.IsAdmin;
            var ok = await _service.UpdateAsync(id, userId!, isAdmin, dto);
            if (!ok) return Forbid();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] int artistProfileId)
        {
            var userId = CurrentUser.UserId;
            if (userId == null) return Unauthorized();
            var ok = await _service.DeleteAsync(id, userId!, artistProfileId);
            if (!ok) return Forbid();
            return NoContent();
        }
    }
}