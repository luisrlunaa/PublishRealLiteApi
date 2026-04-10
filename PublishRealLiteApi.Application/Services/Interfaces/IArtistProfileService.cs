using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IArtistProfileService
    {
        Task<List<ArtistProfileDto>> GetAllAsync();
        Task<ArtistProfileDto?> GetByIdAsync(int id);
        Task<ArtistProfileDto?> CreateAsync(string userId, CreateArtistDto dto);
        Task<bool> UpdateAsync(int id, string userId, bool isAdmin, UpdateArtistDto dto);
        Task<bool> DeleteAsync(int id, string userId, bool isAdmin);
    }
}
