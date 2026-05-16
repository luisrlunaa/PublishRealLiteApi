using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IVideoService
    {
        Task<IEnumerable<ArtistVideo>> GetVideosAsync(int artistProfileId);
        Task<ArtistVideo?> GetVideoByIdAsync(int id, int artistProfileId);
        Task<ArtistVideo> CreateVideoAsync(ArtistVideo video);
        Task<bool> UpdateVideoAsync(ArtistVideo video);
        Task<bool> DeleteVideoAsync(int id, int artistProfileId);
    }
}
