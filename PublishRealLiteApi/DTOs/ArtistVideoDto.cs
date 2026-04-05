namespace PublishRealLiteApi.DTOs
{
    public class ArtistVideoDto
    {
        public int Id { get; set; }
        public int ArtistProfileId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}
