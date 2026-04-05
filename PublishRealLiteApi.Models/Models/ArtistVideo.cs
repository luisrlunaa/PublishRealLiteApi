using System;

namespace PublishRealLiteApi.Models
{
    public class ArtistVideo
    {
        public int Id { get; set; }
        public int ArtistProfileId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty; // imagen <= 3MB
        public string VideoUrl { get; set; } = string.Empty; // YouTube/Vimeo URL
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
