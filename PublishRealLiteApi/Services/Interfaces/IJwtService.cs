using Microsoft.AspNetCore.Identity;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IdentityUser user, IList<string> roles);
    }
}
