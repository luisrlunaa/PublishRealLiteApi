using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Videos;

public static class GetVideosByProfile
{
    public record Query(int ProfileId);

    public record Response(int Id, int ArtistProfileId, string Title, string ThumbnailUrl, string VideoUrl);

    public class Handler(AppDbContext db)
    {
        public async Task<List<Response>> HandleAsync(Query query, CancellationToken ct = default)
        {
            return await db.ArtistVideos
                .Where(v => v.ArtistProfileId == query.ProfileId && !v.IsDeleted)
                .Select(v => new Response(v.Id, v.ArtistProfileId, v.Title, v.ThumbnailUrl, v.VideoUrl))
                .ToListAsync(ct);
        }
    }
}
