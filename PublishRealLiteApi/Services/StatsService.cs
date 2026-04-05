using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Services
{
    public class StatsService : IStatsService
    {
        private readonly AppDbContext _db;
        public StatsService(AppDbContext db) => _db = db;

        public async Task ImportAsync(IEnumerable<StreamStatDto> items)
        {
            var entities = items.Select(d => new StreamStat
            {
                Date = d.Date.Date,
                Platform = d.Platform ?? "Spotify",
                Country = d.Country ?? "Unknown",
                Streams = d.Streams,
                MetricType = d.MetricType ?? "streams",
                Source = d.Source ?? ""
            });
            await _db.StreamStats.AddRangeAsync(entities);
            await _db.SaveChangesAsync();
        }

        public async Task<StatsSummaryDto> GetSummaryAsync(int rangeDays, int? artistProfileId = null)
        {
            var from = DateTime.UtcNow.Date.AddDays(-rangeDays + 1);
            var q = _db.StreamStats.Where(s => s.Date >= from);
            if (artistProfileId.HasValue) q = q.Where(s => s.ArtistProfileId == artistProfileId.Value);

            var total = await q.SumAsync(x => (long)x.Streams);
            var byDate = await q.GroupBy(x => x.Date).Select(g => new ByDateDto { Date = g.Key, Streams = g.Sum(x => x.Streams) }).OrderBy(x => x.Date).ToListAsync();
            var byCountry = await q.GroupBy(x => x.Country).Select(g => new ByCountryDto { Country = g.Key, Streams = g.Sum(x => x.Streams) }).OrderByDescending(x => x.Streams).ToListAsync();
            var bySource = await q.GroupBy(x => x.Source).Select(g => new BySourceDto { Source = g.Key, Streams = g.Sum(x => x.Streams) }).OrderByDescending(x => x.Streams).ToListAsync();

            return new StatsSummaryDto { TotalStreams = total, ByDate = byDate, ByCountry = byCountry, BySource = bySource };
        }

        public Task<IEnumerable<ByCountryDto>> GetByCountryAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<ByCountryDto>>(Array.Empty<ByCountryDto>());
        public Task<IEnumerable<ByDateDto>> GetByDateAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<ByDateDto>>(Array.Empty<ByDateDto>());
        public Task<IEnumerable<BySourceDto>> GetBySourceAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<BySourceDto>>(Array.Empty<BySourceDto>());
    }

}
