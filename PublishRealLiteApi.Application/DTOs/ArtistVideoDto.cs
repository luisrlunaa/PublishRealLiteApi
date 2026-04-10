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

        public ArtistVideoDto() { }

        public ArtistVideoDto(int id, int artistProfileId, string title, string thumbnailUrl, string videoUrl)
        {
            Id = id;
            ArtistProfileId = artistProfileId;
            Title = title;
            ThumbnailUrl = thumbnailUrl;
            VideoUrl = videoUrl;
            CreatedAt = DateTime.UtcNow;
        }
    }

}
