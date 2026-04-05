using PublishRealLiteApi.Infrastructure.Identity;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
