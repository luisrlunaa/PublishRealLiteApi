using System.Security.Claims;

namespace PublishRealLiteApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _ctx;
        public CurrentUserService(IHttpContextAccessor ctx) => _ctx = ctx;

        public string? UserId => _ctx.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public string? UserName => _ctx.HttpContext?.User?.Identity?.Name;
    }
}