using System;

namespace PublishRealLiteApi.Models
{
    public class ArtistApplication
    {
        public int Id { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string InstagramUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? SongAsComposerUrl { get; set; }
        public string? SongAsArtistUrl { get; set; }
        public bool AffiliatedWithPro { get; set; }
        public string OwnershipType { get; set; } = string.Empty;
        public bool InterestedInSigning { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
