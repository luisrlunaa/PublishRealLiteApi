using System;
using System.Collections.Generic;

namespace PublishRealLiteApi.Models
{
    public class Release
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ArtistProfileId { get; set; }
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

        // Soft delete and audit
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = "Published"; // Draft, Pending, Published

        public List<Track> Tracks { get; set; } = new();
    }
}
