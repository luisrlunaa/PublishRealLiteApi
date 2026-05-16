using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Stats;

public static class ImportStats
{
    public class StreamStatDto
    {
        public DateTime Date { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Streams { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    public record Command(IEnumerable<StreamStatDto> Items);

    public record Response(int Imported);

    public class Handler(AppDbContext db)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var entities = cmd.Items.Select(d => new StreamStat
            {
                Date = d.Date.Date,
                Platform = d.Platform ?? "Spotify",
                Country = d.Country ?? "Unknown",
                Streams = d.Streams,
                MetricType = d.MetricType ?? "streams",
                Source = d.Source ?? string.Empty
            }).ToList();

            db.StreamStats.AddRange(entities);
            await db.SaveChangesAsync(ct);

            return new Response(entities.Count);
        }
    }
}
