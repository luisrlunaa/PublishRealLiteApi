using Microsoft.AspNetCore.Identity;

namespace PublishRealLiteApi.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public int? ArtistProfileId { get; set; }
}
