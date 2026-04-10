using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services
{
    public class ArtistVideoService : IArtistVideoService
    {
        private readonly IArtistVideoRepository _repo;
        private readonly IArtistProfileRepository _artistRepo;
        public ArtistVideoService(IArtistVideoRepository repo, IArtistProfileRepository artistRepo)
        {
            _repo = repo;
            _artistRepo = artistRepo;
        }

        public async Task<List<ArtistVideoDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.ConvertAll(v => new ArtistVideoDto(v.Id, v.ArtistProfileId, v.Title, v.ThumbnailUrl, v.VideoUrl));
        }

        public async Task<ArtistVideoDto?> GetByIdAsync(int id)
        {
            var v = await _repo.GetByIdAsync(id);
            return v == null ? null : new ArtistVideoDto(v.Id, v.ArtistProfileId, v.Title, v.ThumbnailUrl, v.VideoUrl);
        }

        public async Task<List<ArtistVideoDto>> GetByArtistProfileIdAsync(int artistProfileId)
        {
            var list = await _repo.GetByArtistProfileIdAsync(artistProfileId);
            return list.ConvertAll(v => new ArtistVideoDto(v.Id, v.ArtistProfileId, v.Title, v.ThumbnailUrl, v.VideoUrl));
        }

        public async Task<ArtistVideoDto> CreateAsync(CreateArtistVideoDto dto, string userId, bool isAdmin)
        {
            var profile = await _artistRepo.GetByIdAsync(dto.ArtistProfileId);
            if (profile == null) throw new KeyNotFoundException();
            if (!isAdmin && profile.UserId != userId) throw new UnauthorizedAccessException();
            var video = new ArtistVideo
            {
                ArtistProfileId = dto.ArtistProfileId,
                Title = dto.Title,
                ThumbnailUrl = dto.ThumbnailUrl,
                VideoUrl = dto.VideoUrl
            };
            await _repo.AddAsync(video);
            return new ArtistVideoDto(video.Id, video.ArtistProfileId, video.Title, video.ThumbnailUrl, video.VideoUrl);
        }

        public async Task<bool> UpdateAsync(int id, UpdateArtistVideoDto dto, string userId, bool isAdmin)
        {
            var video = await _repo.GetByIdAsync(id);
            if (video == null) return false;
            var profile = await _artistRepo.GetByIdAsync(video.ArtistProfileId);
            if (!isAdmin && profile?.UserId != userId) return false;
            video.Title = dto.Title;
            video.ThumbnailUrl = dto.ThumbnailUrl;
            video.VideoUrl = dto.VideoUrl;
            await _repo.UpdateAsync(video);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var video = await _repo.GetByIdAsync(id);
            if (video == null) return false;
            var profile = await _artistRepo.GetByIdAsync(video.ArtistProfileId);
            if (!isAdmin && profile?.UserId != userId) return false;
            await _repo.DeleteAsync(video);
            return true;
        }
    }
}
