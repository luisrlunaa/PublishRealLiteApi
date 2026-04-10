using PublishRealLiteApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface IArtistVideoRepository
    {
        Task<List<ArtistVideo>> GetAllAsync();
        Task<ArtistVideo?> GetByIdAsync(int id);
        Task AddAsync(ArtistVideo video);
        Task UpdateAsync(ArtistVideo video);
        Task DeleteAsync(ArtistVideo video);
        Task<List<ArtistVideo>> GetByArtistProfileIdAsync(int artistProfileId);
    }
}
