namespace PublishRealLiteApi.DTOs
{
    public record ArtistProfileDto(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile = true);
    public record UpdateArtistDto(string ArtistName, string? Bio, string? SocialLinksJson);
    public record CreateArtistDto(string ArtistName, string? Bio, string? SocialLinksJson);

    // For creating a profile with admin code
    public record CreateArtistWithAdminCodeDto(string ArtistName, string? Bio, string? SocialLinksJson, string? AdminUserId);

    // Response for admin profile with subordinate profiles
    public record AdminProfileResponseDto(
        int Id,
        string UserId,
        string ArtistName,
        string? Bio,
        string? ProfileImageUrl,
        string? SocialLinksJson,
        string UserIdForInvite,
        List<ArtistProfileDto> SubProfiles
    );
}
