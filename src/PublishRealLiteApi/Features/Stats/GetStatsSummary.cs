using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Stats;

public static class GetStatsSummary
{
    public record Query(int RangeDays, int? ArtistProfileId = null);

    public record ByDateDto(DateTime Date, int Streams);
    public record ByCountryDto(string Country, int Streams);
    public record BySourceDto(string Source, int Streams);

    public record Response(
        long TotalStreams,
        List<ByDateDto> ByDate,
        List<ByCountryDto> ByCountry,
        List<BySourceDto> BySource);

    public class Handler(AppDbContext db)
    {
        public async Task<Response> HandleAsync(Query query, CancellationToken ct = default)
        {
            var from = DateTime.UtcNow.Date.AddDays(-query.RangeDays + 1);
            var to = DateTime.UtcNow.Date;

            var statsQuery = db.StreamStats
                .Where(s => !s.IsDeleted && s.Date >= from && s.Date <= to);

            if (query.ArtistProfileId.HasValue)
                statsQuery = statsQuery.Where(s => s.ArtistProfileId == query.ArtistProfileId.Value);

            var stats = await statsQuery.ToListAsync(ct);

            var total = stats.Sum(x => (long)x.Streams);

            var byDate = stats
                .GroupBy(x => x.Date)
                .Select(g => new ByDateDto(g.Key, g.Sum(x => x.Streams)))
                .OrderBy(x => x.Date)
                .ToList();

            var byCountry = stats
                .GroupBy(x => x.Country)
                .Select(g => new ByCountryDto(g.Key, g.Sum(x => x.Streams)))
                .OrderByDescending(x => x.Streams)
                .ToList();

            var bySource = stats
                .GroupBy(x => x.Source)
                .Select(g => new BySourceDto(g.Key, g.Sum(x => x.Streams)))
                .OrderByDescending(x => x.Streams)
                .ToList();

            return new Response(total, byDate, byCountry, bySource);
        }
    }
}
