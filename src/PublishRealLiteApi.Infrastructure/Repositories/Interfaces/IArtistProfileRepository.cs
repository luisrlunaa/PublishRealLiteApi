using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Repositories.Interfaces
{
    public interface IArtistProfileRepository
    {
        Task<List<ArtistProfile>> GetAllAsync();
        Task<ArtistProfile?> GetByIdAsync(Guid id);
        Task<ArtistProfile?> GetByUserIdAsync(string userId);
        Task AddAsync(ArtistProfile profile);
        Task UpdateAsync(ArtistProfile profile);
        Task DeleteAsync(ArtistProfile profile);
        Task<bool> ExistsForUserAsync(string userId);
    }
}
