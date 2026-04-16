using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IArtistProfileService
    {
        Task<List<ArtistProfileDto>> GetAllAsync();
        Task<ArtistProfileDto?> GetByIdAsync(int id);
        Task<ArtistProfileDto?> CreateAsync(string userId, CreateArtistDto dto);
        Task<ArtistProfileDto?> CreateWithAdminCodeAsync(string userId, CreateArtistWithAdminCodeDto dto);
        Task<AdminProfileResponseDto?> GetAdminProfileWithSubProfilesAsync(string userId);
        Task<List<ArtistProfileDto>> GetSubProfilesAsync(string adminUserId);
        Task<bool> UpdateAsync(int id, string userId, bool isAdmin, UpdateArtistDto dto);
        Task<bool> DeleteAsync(int id, string userId, bool isAdmin);
    }
}
