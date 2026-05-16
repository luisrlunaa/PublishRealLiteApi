using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class GetArtistProfileById
{
    public record Query(int Id);

    public record Response(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile);

    public class Handler(AppDbContext db)
    {
        public async Task<Response?> HandleAsync(Query query, CancellationToken ct = default)
        {
            var p = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.Id == query.Id && !p.IsDeleted, ct);

            if (p == null) return null;

            return new Response(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile);
        }
    }
}
