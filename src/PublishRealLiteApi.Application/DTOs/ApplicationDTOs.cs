namespace PublishRealLiteApi.Application.DTOs
{
    public record SubmitApplicationDto(
        string ArtistName,
        string Email,
        string Country,
        string InstagramUrl,
        string Role,
        string? SongAsComposerUrl,
        string? SongAsArtistUrl,
        bool AffiliatedWithPro,
        string OwnershipType,
        bool InterestedInSigning,
        string TurnstileToken
    );
}
