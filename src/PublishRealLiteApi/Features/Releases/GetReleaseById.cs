using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Releases;

public static class GetReleaseById
{
    public record Query(Guid Id);

    public record TrackDto(Guid Id, Guid ReleaseId, int Position, string Title);

    public record Response(Guid Id, int ArtistProfileId, string Title, DateTime? ReleaseDate, string? Genre, string? Label, string? UPC, string? ISRC, string? LinksJson, List<TrackDto> Tracks);

    public class Handler(AppDbContext db)
    {
        public async Task<Response?> HandleAsync(Query query, CancellationToken ct = default)
        {
            var r = await db.Releases
                .Include(r => r.Tracks)
                .FirstOrDefaultAsync(r => r.Id == query.Id && !r.IsDeleted, ct);

            if (r == null) return null;

            return new Response(
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
            );
        }
    }
}
