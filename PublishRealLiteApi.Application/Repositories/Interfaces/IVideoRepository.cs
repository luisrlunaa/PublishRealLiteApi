using PublishRealLiteApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface IVideoRepository
    {
        Task<IEnumerable<ArtistVideo>> GetByArtistAsync(int artistProfileId);
        Task<ArtistVideo?> GetByIdAsync(int id, int artistProfileId);
        Task<ArtistVideo> AddAsync(ArtistVideo video);
        Task<bool> UpdateAsync(ArtistVideo video);
        Task<bool> DeleteAsync(int id, int artistProfileId);
    }
}
