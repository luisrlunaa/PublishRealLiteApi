using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Tracks;

public static class GetTracksByRelease
{
    public record Query(Guid ReleaseId);

    public record Response(Guid Id, Guid ReleaseId, int Position, string Title);

    public class Handler(AppDbContext db)
    {
        public async Task<List<Response>> HandleAsync(Query query, CancellationToken ct = default)
        {
            return await db.Tracks
                .Where(t => t.ReleaseId == query.ReleaseId && !t.IsDeleted)
                .OrderBy(t => t.Position)
                .Select(t => new Response(t.Id, t.ReleaseId, t.Position, t.Title))
                .ToListAsync(ct);
        }
    }
}
