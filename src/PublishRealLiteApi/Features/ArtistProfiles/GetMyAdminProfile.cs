using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class GetMyAdminProfile
{
    public record Query(string UserId);

    public record SubProfileDto(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile);

    public record Response(
        int Id,
        string UserId,
        string ArtistName,
        string? Bio,
        string? ProfileImageUrl,
        string? SocialLinksJson,
        string UserIdForInvite,
        List<SubProfileDto> SubProfiles);

    public class Handler(AppDbContext db)
    {
        public async Task<Response?> HandleAsync(Query query, CancellationToken ct = default)
        {
            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.UserId == query.UserId && p.IsAdminProfile && !p.IsDeleted, ct);

            if (profile == null) return null;

            var subProfiles = await db.ArtistProfiles
                .Where(p => p.AdminUserId == query.UserId && !p.IsDeleted)
                .Select(p => new SubProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile))
                .ToListAsync(ct);

            return new Response(
                profile.Id,
                profile.UserId,
                profile.ArtistName,
                profile.Bio,
                profile.ProfileImageUrl,
                profile.SocialLinksJson,
                profile.UserId,
                subProfiles);
        }
    }
}
