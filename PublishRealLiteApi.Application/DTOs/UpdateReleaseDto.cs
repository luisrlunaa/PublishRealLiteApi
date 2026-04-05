using System;

namespace PublishRealLiteApi.DTOs
{
    public record UpdateReleaseDto(
        string Title,
        DateTime? ReleaseDate,
        string? Genre,
        string? Label,
        string? UPC,
        string? ISRC,
        string? LinksJson
    );
}
