namespace PublishRealLiteApi.DTOs
{
    public record CreateReleaseDto(
        int ArtistProfileId,
        string Title,
        DateTime? ReleaseDate,
        string? Genre,
        string? Label,
        string? UPC,
        string? ISRC,
        string? LinksJson
    );
}
