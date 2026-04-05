using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
