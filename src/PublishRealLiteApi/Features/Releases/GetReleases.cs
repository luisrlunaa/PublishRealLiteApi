using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Releases;

public static class GetReleases
{
    public record Query(int ArtistProfileId);

    public record TrackDto(Guid Id, Guid ReleaseId, int Position, string Title);

    public record Response(Guid Id, int ArtistProfileId, string Title, DateTime? ReleaseDate, string? Genre, string? Label, string? UPC, string? ISRC, string? LinksJson, List<TrackDto> Tracks);

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<List<Response>> HandleAsync(Query query, CancellationToken ct = default)
        {
            var userId = currentUser.UserId;
            var isAdmin = currentUser.IsAdmin;

            var q = db.Releases
                .Include(r => r.Tracks)
                .Where(r => !r.IsDeleted);

            if (isAdmin)
                q = q.Where(r => r.CreatedBy == userId);
            else
                q = q.Where(r => r.ArtistProfileId == query.ArtistProfileId);

            var releases = await q.ToListAsync(ct);

            return releases.Select(r => new Response(
                r.Id,
                r.ArtistProfileId,
                r.Title,
                r.ReleaseDate,
                r.Genre,
                r.Label,
                r.UPC,
                r.ISRC,
                r.LinksJson,
                r.Tracks.Select(t => new TrackDto(t.Id, t.ReleaseId, t.Position, t.Title)).ToList()
            )).ToList();
        }
    }
}
