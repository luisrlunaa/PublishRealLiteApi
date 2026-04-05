namespace PublishRealLiteApi.Models
{
    public class Release
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ArtistProfileId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }

        public string Title { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? Genre { get; set; }
        public string? Label { get; set; }
        public string? UPC { get; set; }
        public string? ISRC { get; set; }
        public string? LinksJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Track> Tracks { get; set; } = new();
    }
}
