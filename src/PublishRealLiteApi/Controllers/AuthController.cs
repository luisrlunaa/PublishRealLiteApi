using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ITurnstileService _turnstile;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IJwtService jwtService, ITurnstileService turnstile)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _turnstile = turnstile;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!await _turnstile.ValidateAsync(dto.TurnstileToken))
                return BadRequest(new { message = "Something went wrong, please try again." });

            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, "Artist");
            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!await _turnstile.ValidateAsync(dto.TurnstileToken))
                return BadRequest(new { message = "Something went wrong, please try again." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new AuthResponseDto(token, user.Email));
        }
    }
}