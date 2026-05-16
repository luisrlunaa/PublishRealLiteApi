using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Stats.Workers;

public interface IStreamStatAggregatorService
{
    Task AggregateOnceAsync(CancellationToken ct = default);
}

public class StreamStatAggregatorService(IServiceProvider services, ILogger<StreamStatAggregatorService> logger) : IStreamStatAggregatorService
{
    public async Task AggregateOnceAsync(CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var raws = db.StreamStats.Where(s => !s.Aggregated && !s.IsDeleted).ToList();
        if (!raws.Any())
        {
            logger.LogDebug("No new stream stats to aggregate (manual run)");
            return;
        }

        var groups = raws
            .GroupBy(r => new { r.ArtistProfileId, r.ReleaseId, r.Date, r.Platform })
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
            var existing = db.StreamStatDailyAggregates.FirstOrDefault(a =>
                a.ArtistProfileId == g.ArtistProfileId &&
                a.ReleaseId == g.ReleaseId &&
                a.Date == g.Date &&
                a.Platform == g.Platform);

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

            var rawsToMark = db.StreamStats.Where(s => g.RawIds.Contains(s.Id)).ToList();
            foreach (var r in rawsToMark)
            {
                r.Aggregated = true;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Manual aggregation completed: {Count} groups", groups.Count);
    }
}

public class StreamStatAggregatorWorker(IServiceProvider services, ILogger<StreamStatAggregatorWorker> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("StreamStatAggregatorWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AggregateAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during aggregate run");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task AggregateAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var raws = db.StreamStats.Where(s => !s.Aggregated && !s.IsDeleted).ToList();
        if (!raws.Any())
        {
            logger.LogDebug("No new stream stats to aggregate");
            return;
        }

        var groups = raws
            .GroupBy(r => new { r.ArtistProfileId, r.ReleaseId, r.Date, r.Platform })
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
            var existing = db.StreamStatDailyAggregates.FirstOrDefault(a =>
                a.ArtistProfileId == g.ArtistProfileId &&
                a.ReleaseId == g.ReleaseId &&
                a.Date == g.Date &&
                a.Platform == g.Platform);

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

            var rawsToMark = db.StreamStats.Where(s => g.RawIds.Contains(s.Id)).ToList();
            foreach (var r in rawsToMark)
            {
                r.Aggregated = true;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Aggregated {Count} raw stat groups", groups.Count);
    }
}
