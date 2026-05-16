using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class GetArtistProfiles
{
    public record Query();

    public record Response(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile);

    public class Handler(AppDbContext db)
    {
        public async Task<List<Response>> HandleAsync(Query query, CancellationToken ct = default)
        {
            return await db.ArtistProfiles
                .Where(p => !p.IsDeleted)
                .Select(p => new Response(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile))
                .ToListAsync(ct);
        }
    }
}
