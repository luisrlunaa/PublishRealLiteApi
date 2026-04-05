namespace PublishRealLiteApi.DTOs
{
    public record CreateReleaseDto(
        Guid ArtistProfileId,
        string Title,
        DateTime? ReleaseDate,
        string? Genre,
        string? Label,
        string? UPC,
        string? ISRC,
        string? LinksJson
    );
}
