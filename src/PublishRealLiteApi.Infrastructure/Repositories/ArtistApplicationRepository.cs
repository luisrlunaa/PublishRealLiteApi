using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Repositories
{
    public class ArtistApplicationRepository : IArtistApplicationRepository
    {
        private readonly AppDbContext _db;
        public ArtistApplicationRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(ArtistApplication application)
        {
            _db.ArtistApplications.Add(application);
            await _db.SaveChangesAsync();
        }
    }
}
