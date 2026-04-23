using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Persistence.Repositories
{
    public class StatsRepository : IStatsRepository
    {
        private readonly AppDbContext _db;

        public StatsRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<StreamStat>> GetStatsAsync(DateTime from, DateTime to, int? artistProfileId = null, int? releaseId = null)
        {
            var q = _db.StreamStats.AsQueryable();

            if (artistProfileId.HasValue) q = q.Where(s => s.ArtistProfileId == artistProfileId.Value && !s.IsDeleted);
            if (releaseId.HasValue && releaseId.Value > 0) q = q.Where(s => s.ReleaseId == releaseId.Value && !s.IsDeleted);

            q = q.Where(s => s.Date >= from && s.Date <= to);

            return await q.ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<StreamStat> stats)
        {
            _db.StreamStats.AddRange(stats);
            await _db.SaveChangesAsync();
        }
    }
}
