using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Videos;

public static class GetMyVideos
{
    public record Query();

    public record Response(int Id, int ArtistProfileId, string Title, string ThumbnailUrl, string VideoUrl);

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<List<Response>?> HandleAsync(Query query, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);

            if (profile == null) return null;

            return await db.ArtistVideos
                .Where(v => v.ArtistProfileId == profile.Id && !v.IsDeleted)
                .Select(v => new Response(v.Id, v.ArtistProfileId, v.Title, v.ThumbnailUrl, v.VideoUrl))
                .ToListAsync(ct);
        }
    }
}
