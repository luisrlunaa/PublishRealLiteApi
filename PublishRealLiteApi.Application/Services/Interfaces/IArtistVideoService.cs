using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IArtistVideoService
    {
        Task<List<ArtistVideoDto>> GetAllAsync();
        Task<ArtistVideoDto?> GetByIdAsync(int id);
        Task<List<ArtistVideoDto>> GetByArtistProfileIdAsync(int artistProfileId);
        Task<ArtistVideoDto> CreateAsync(CreateArtistVideoDto dto, string userId, bool isAdmin);
        Task<bool> UpdateAsync(int id, UpdateArtistVideoDto dto, string userId, bool isAdmin);
        Task<bool> DeleteAsync(int id, string userId, bool isAdmin, int artistProfileId);
    }
}
