using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface IReleaseRepository
    {
        Task<IEnumerable<Release>> GetByArtistAsync(string userId);
        Task<IEnumerable<Release>> GetByArtistAsync(int artistProfileId);
        Task<Release?> GetByIdAsync(Guid id, int artistProfileId);
        Task<Release?> GetByIdAsync(Guid id);
        Task<Release> AddAsync(Release release);
        Task<bool> UpdateAsync(Release release);
        Task<bool> DeleteAsync(Guid id, int artistProfileId);
    }
}
