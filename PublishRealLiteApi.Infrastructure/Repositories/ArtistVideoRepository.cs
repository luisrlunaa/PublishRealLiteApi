using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Repositories
{
    public class ArtistVideoRepository : IArtistVideoRepository
    {
        private readonly AppDbContext _db;
        public ArtistVideoRepository(AppDbContext db) => _db = db;

        public async Task<List<ArtistVideo>> GetAllAsync() =>
            await _db.ArtistVideos.Where(x=> !x.IsDeleted).ToListAsync();

        public async Task<ArtistVideo?> GetByIdAsync(int id) =>
            await _db.ArtistVideos.FirstOrDefaultAsync(x=>x.Id==id && !x.IsDeleted);

        public async Task AddAsync(ArtistVideo video)
        {
            _db.ArtistVideos.Add(video);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ArtistVideo video)
        {
            _db.ArtistVideos.Update(video);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ArtistVideo video)
        {
            _db.ArtistVideos.Remove(video);
            await _db.SaveChangesAsync();
        }

        public async Task<List<ArtistVideo>> GetByArtistProfileIdAsync(int artistProfileId) =>
            await _db.ArtistVideos.Where(v => v.ArtistProfileId == artistProfileId && !v.IsDeleted).ToListAsync();
    }
}
