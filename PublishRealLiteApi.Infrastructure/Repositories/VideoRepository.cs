using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Persistence.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly AppDbContext _db;

        public VideoRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<ArtistVideo>> GetByArtistAsync(int artistProfileId)
        {
            return await _db.ArtistVideos
                .Where(v => v.ArtistProfileId == artistProfileId)
                .ToListAsync();
        }

        public async Task<ArtistVideo?> GetByIdAsync(int id, int artistProfileId)
        {
            return await _db.ArtistVideos
                .FirstOrDefaultAsync(v => v.Id == id && v.ArtistProfileId == artistProfileId);
        }

        public async Task<ArtistVideo> AddAsync(ArtistVideo video)
        {
            _db.ArtistVideos.Add(video);
            await _db.SaveChangesAsync();
            return video;
        }

        public async Task<bool> UpdateAsync(ArtistVideo video)
        {
            _db.ArtistVideos.Update(video);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, int artistProfileId)
        {
            var entity = await GetByIdAsync(id, artistProfileId);
            if (entity == null) return false;
            _db.ArtistVideos.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
