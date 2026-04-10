using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Repositories
{
    public class ArtistProfileRepository : IArtistProfileRepository
    {
        private readonly AppDbContext _db;
        public ArtistProfileRepository(AppDbContext db) => _db = db;

        public async Task<List<ArtistProfile>> GetAllAsync() =>
            await _db.ArtistProfiles.ToListAsync();

        public async Task<ArtistProfile?> GetByIdAsync(int id) =>
            await _db.ArtistProfiles.FindAsync(id);

        public async Task<ArtistProfile?> GetByUserIdAsync(string userId) =>
            await _db.ArtistProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        public async Task AddAsync(ArtistProfile profile)
        {
            _db.ArtistProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ArtistProfile profile)
        {
            _db.ArtistProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ArtistProfile profile)
        {
            _db.ArtistProfiles.Remove(profile);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsForUserAsync(string userId) =>
            await _db.ArtistProfiles.AnyAsync(x => x.UserId == userId);
    }
}
