using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;

        public VideoService(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        public async Task<IEnumerable<ArtistVideo>> GetVideosAsync(int artistProfileId)
        {
            return await _videoRepository.GetByArtistAsync(artistProfileId);
        }

        public async Task<ArtistVideo?> GetVideoByIdAsync(int id, int artistProfileId)
        {
            return await _videoRepository.GetByIdAsync(id, artistProfileId);
        }

        public async Task<ArtistVideo> CreateVideoAsync(ArtistVideo video)
        {
            return await _videoRepository.AddAsync(video);
        }

        public async Task<bool> UpdateVideoAsync(ArtistVideo video)
        {
            return await _videoRepository.UpdateAsync(video);
        }

        public async Task<bool> DeleteVideoAsync(int id, int artistProfileId)
        {
            return await _videoRepository.DeleteAsync(id, artistProfileId);
        }
    }
}
