using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Workers
{
    public class StreamStatAggregatorWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<StreamStatAggregatorWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public StreamStatAggregatorWorker(IServiceProvider services, ILogger<StreamStatAggregatorWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StreamStatAggregatorWorker started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AggregateAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during aggregate run");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task AggregateAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Process un-aggregated stats grouped by day/platform/artist
            var raws = db.StreamStats.Where(s => !s.Aggregated && !s.IsDeleted).ToList();
            if (!raws.Any())
            {
                _logger.LogDebug("No new stream stats to aggregate");
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
                    db.StreamStatDailyAggregates.Add(new StreamStatDailyAggregate
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

                // Mark raws as aggregated
                var rawsToMark = db.StreamStats.Where(s => g.RawIds.Contains(s.Id)).ToList();
                foreach (var r in rawsToMark)
                {
                    r.Aggregated = true;
                }
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Aggregated {Count} raw stat groups", groups.Count);
        }
    }
}
