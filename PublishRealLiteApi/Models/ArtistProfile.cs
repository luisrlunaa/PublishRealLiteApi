namespace PublishRealLiteApi.Models
{
    public class ArtistProfile
    {
        public GuidId { get; set; } = Guid.NewGuid();

        // FK to Identity user (string Id) 
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        public string ArtistName { get; set; } = string.Empty;
        publicstring? Bio { get; set; }
        publicstring? ProfileImageUrl { get; set; }
        publicstring? SocialLinksJson { get; set; } // JSON string for flexible links 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Release> Releases { get; set; } = new();
    }
}