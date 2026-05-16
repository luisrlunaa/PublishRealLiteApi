using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PublishRealLiteApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(IdentityUser user, IList<string> roles)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secret = jwtSettings.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT secret missing");
            var issuer = jwtSettings.GetValue<string>("Issuer") ?? "PublishRealLiteApi";
            var audience = jwtSettings.GetValue<string>("Audience") ?? "PublishRealLiteApiClients";
            var expiryMinutes = jwtSettings.GetValue<int?>("ExpiryMinutes") ?? 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
