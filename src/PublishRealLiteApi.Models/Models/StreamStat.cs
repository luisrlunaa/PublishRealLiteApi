using System;

namespace PublishRealLiteApi.Models
{
    public class StreamStat
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } // data date (day) 
        public string Platform { get; set; } = "Spotify";
        public string Country { get; set; } = "Unknown";
        public int Streams { get; set; }
        public string MetricType { get; set; } = "streams"; // e.g., streams, listeners 
        public string Source { get; set; } = ""; // discovery source, playlist, radio, etc. 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ArtistProfileId { get; set; }
        public int? ReleaseId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }

        // Audit/soft-delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        // Aggregation flag: indicates this raw stat has been processed into daily aggregates
        public bool Aggregated { get; set; } = false;
    }
}