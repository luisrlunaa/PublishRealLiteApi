namespace PublishRealLiteApi.DTOs
{
    public record CreateArtistVideoDto(int ArtistProfileId, string Title, string ThumbnailUrl, string VideoUrl);
}
