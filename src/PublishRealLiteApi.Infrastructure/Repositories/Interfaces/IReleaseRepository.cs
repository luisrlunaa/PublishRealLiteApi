using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Repositories.Interfaces
{
    public interface IReleaseRepository
    {
        Task<List<Release>> GetAllAsync();
        Task<Release?> GetByIdAsync(Guid id);
        Task AddAsync(Release release);
        Task UpdateAsync(Release release);
        Task DeleteAsync(Release release);
    }
}
