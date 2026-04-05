namespace PublishRealLiteApi.DTOs
{
    public record ArtistProfileDto(Guid Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson);
    public record CreateArtistDto(string ArtistName, string? Bio, string? SocialLinksJson);
    public record UpdateArtistDto(string ArtistName, string? Bio, string? SocialLinksJson);
}
