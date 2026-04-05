using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
