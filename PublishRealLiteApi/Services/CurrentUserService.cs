using Microsoft.AspNetCore.Http;
using PublishRealLiteApi.Application.Services.Interfaces;
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