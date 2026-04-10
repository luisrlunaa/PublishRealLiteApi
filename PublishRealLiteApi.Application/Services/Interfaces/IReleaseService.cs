using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<IEnumerable<ReleaseDto>> GetAllAsync();
        Task<ReleaseDto?> GetByIdAsync(Guid id);
        Task<ReleaseDto?> CreateAsync(string userId, bool isAdmin, CreateReleaseDto dto);
        Task<bool> UpdateAsync(Guid id, string userId, bool isAdmin, UpdateReleaseDto dto);
        Task<bool> DeleteAsync(Guid id, string userId, bool isAdmin);
    }
}
