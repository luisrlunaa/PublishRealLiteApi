using System.Security.Claims;

namespace PublishRealLiteApi.Common;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("name");

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public bool IsAdmin =>
        User?.IsInRole("Admin") == true
        || User?.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin") == true;
}
