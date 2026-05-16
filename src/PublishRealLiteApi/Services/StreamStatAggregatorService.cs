using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Services
{
    public class StreamStatAggregatorService : PublishRealLiteApi.Services.Interfaces.IStreamStatAggregatorService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<StreamStatAggregatorService> _logger;

        public StreamStatAggregatorService(IServiceProvider services, ILogger<StreamStatAggregatorService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task AggregateOnceAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var raws = db.StreamStats.Where(s => !s.Aggregated && !s.IsDeleted).ToList();
            if (!raws.Any())
            {
                _logger.LogDebug("No new stream stats to aggregate (manual run)");
                return;
            }

            var groups = raws.GroupBy(r => new { r.ArtistProfileId, r.ReleaseId, r.Date, r.Platform })
                .Select(g => new
                {
                    g.Key.ArtistProfileId,
                    g.Key.ReleaseId,
                    g.Key.Date,
                    g.Key.Platform,
                    Streams = g.Sum(x => x.Streams),
                    RawIds = g.Select(x => x.Id).ToList()
                })
                .ToList();

            foreach (var g in groups)
            {
                var existing = db.StreamStatDailyAggregates.FirstOrDefault(a => a.ArtistProfileId == g.ArtistProfileId && a.ReleaseId == g.ReleaseId && a.Date == g.Date && a.Platform == g.Platform);
                if (existing == null)
                {
                    db.StreamStatDailyAggregates.Add(new PublishRealLiteApi.Models.StreamStatDailyAggregate
                    {
                        ArtistProfileId = g.ArtistProfileId,
                        ReleaseId = g.ReleaseId,
                        Date = g.Date,
                        Platform = g.Platform,
                        Streams = g.Streams
                    });
                }
                else
                {
                    existing.Streams += g.Streams;
                }

                var rawsToMark = db.StreamStats.Where(s => g.RawIds.Contains(s.Id)).ToList();
                foreach (var r in rawsToMark)
                {
                    r.Aggregated = true;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Manual aggregation completed: {Count} groups", groups.Count);
        }
    }
}
