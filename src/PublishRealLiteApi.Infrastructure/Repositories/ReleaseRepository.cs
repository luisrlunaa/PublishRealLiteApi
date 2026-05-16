using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Persistence.Repositories
{
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly AppDbContext _db;

        public ReleaseRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Release>> GetByArtistAsync(string userId)
        {
            return await _db.Releases
                .Where(r => r.CreatedBy == userId && !r.IsDeleted)
                .Include(r => r.Tracks)
                .ToListAsync();
        }


        public async Task<IEnumerable<Release>> GetByArtistAsync(int artistProfileId)
        {
            return await _db.Releases
                .Where(r => r.ArtistProfileId == artistProfileId && !r.IsDeleted)
                .Include(r => r.Tracks)
                .ToListAsync();
        }

        public async Task<Release?> GetByIdAsync(Guid id, int artistProfileId)
        {
            return await _db.Releases
                .Include(r => r.Tracks)
                .FirstOrDefaultAsync(r => r.Id == id && r.ArtistProfileId == artistProfileId && !r.IsDeleted);
        }

        public async Task<Release?> GetByIdAsync(Guid id)
        {
            return await _db.Releases
                .Include(r => r.Tracks)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Release> AddAsync(Release release)
        {
            _db.Releases.Add(release);
            await _db.SaveChangesAsync();
            return release;
        }

        public async Task<bool> UpdateAsync(Release release)
        {
            _db.Releases.Update(release);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, int artistProfileId)
        {
            var entity = await GetByIdAsync(id, artistProfileId);
            if (entity == null) return false;
            _db.Releases.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
