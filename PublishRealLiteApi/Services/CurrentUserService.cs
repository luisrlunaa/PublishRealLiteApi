using PublishRealLiteApi.Services.Interfaces;
using System.Security.Claims;


namespace PublishRealLiteApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User?.FindFirstValue("sub");

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name) ??
        User?.Identity?.Name;

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAdmin
    {
        get
        {
            if (User == null) return false;
            // Comprueba ClaimTypes.Role y también "role" por compatibilidad con JWT
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
                        .Concat(User.FindAll("role").Select(c => c.Value));
            return roles.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(r, "administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
