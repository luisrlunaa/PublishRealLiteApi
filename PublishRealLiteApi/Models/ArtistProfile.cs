namespace PublishRealLiteApi.Models
{
    public class ArtistProfile
    {
        public int Id { get; set; }

        // FK to Identity user (string Id) 
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SocialLinksJson { get; set; } // JSON string for flexible links 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Release> Releases { get; set; } = new List<Release>();
        public ICollection<ArtistVideo> Videos { get; set; } = new List<ArtistVideo>();
    }
}