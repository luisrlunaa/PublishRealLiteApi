using System;

namespace PublishRealLiteApi.DTOs
{
    public record ReleaseDto(
        Guid Id,
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
