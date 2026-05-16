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
        // Soft delete and audit
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
