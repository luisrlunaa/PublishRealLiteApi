using Microsoft.AspNetCore.Identity;

namespace PublishRealLiteApi.Models
{
    // Extends IdentityUser for future properties
    public class AppUser : IdentityUser
    {
        // Additional user properties can be added here
        public ArtistProfile? ArtistProfile { get; set; }
    }
}