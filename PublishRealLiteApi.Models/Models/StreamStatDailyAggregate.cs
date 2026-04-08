using System;

namespace PublishRealLiteApi.Models
{
    public class StreamStatDailyAggregate
    {
        public int Id { get; set; }
        public int? ArtistProfileId { get; set; }
        public int? ReleaseId { get; set; }
        public DateTime Date { get; set; }
        public string Platform { get; set; } = "Spotify";
        public long Streams { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
