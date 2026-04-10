using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Services
{
    public class StatsService : IStatsService
    {
        private readonly IStatsRepository _statsRepository;

        public StatsService(IStatsRepository statsRepository)
        {
            _statsRepository = statsRepository;
        }

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
            await _statsRepository.AddRangeAsync(entities);
        }

        public async Task<StatsSummaryDto> GetSummaryAsync(int rangeDays, int? artistProfileId = null)
        {
            // Delega la obtención de datos al repositorio; el repositorio debe exponer métodos que permitan filtrar por fecha y artista.
            var from = DateTime.UtcNow.Date.AddDays(-rangeDays + 1);
            var to = DateTime.UtcNow.Date;
            var stats = await _statsRepository.GetStatsAsync(from, to, artistProfileId);

            var total = stats.Sum(x => (long)x.Streams);
            var byDate = stats.GroupBy(x => x.Date).Select(g => new ByDateDto { Date = g.Key, Streams = g.Sum(x => x.Streams) }).OrderBy(x => x.Date).ToList();
            var byCountry = stats.GroupBy(x => x.Country).Select(g => new ByCountryDto { Country = g.Key, Streams = g.Sum(x => x.Streams) }).OrderByDescending(x => x.Streams).ToList();
            var bySource = stats.GroupBy(x => x.Source).Select(g => new BySourceDto { Source = g.Key, Streams = g.Sum(x => x.Streams) }).OrderByDescending(x => x.Streams).ToList();

            return new StatsSummaryDto { TotalStreams = total, ByDate = byDate, ByCountry = byCountry, BySource = bySource };
        }

        public Task<IEnumerable<ByCountryDto>> GetByCountryAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<ByCountryDto>>(Array.Empty<ByCountryDto>());
        public Task<IEnumerable<ByDateDto>> GetByDateAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<ByDateDto>>(Array.Empty<ByDateDto>());
        public Task<IEnumerable<BySourceDto>> GetBySourceAsync(int rangeDays, int? artistProfileId = null) => Task.FromResult<IEnumerable<BySourceDto>>(Array.Empty<BySourceDto>());
    }
}
