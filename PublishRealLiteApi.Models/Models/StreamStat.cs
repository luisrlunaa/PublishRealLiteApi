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
    }
}