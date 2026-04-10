using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Services
{
    public class ArtistProfileService : IArtistProfileService
    {
        private readonly IArtistProfileRepository _repo;
        public ArtistProfileService(IArtistProfileRepository repo) => _repo = repo;

        public async Task<List<ArtistProfileDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.ConvertAll(p => new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson));
        }

        public async Task<ArtistProfileDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p == null ? null : new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson);
        }

        public async Task<ArtistProfileDto?> CreateAsync(string userId, CreateArtistDto dto)
        {
            if (await _repo.ExistsForUserAsync(userId)) return null;
            var profile = new ArtistProfile
            {
                UserId = userId,
                ArtistName = dto.ArtistName,
                Bio = dto.Bio,
                SocialLinksJson = dto.SocialLinksJson
            };
            await _repo.AddAsync(profile);
            return new ArtistProfileDto(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson);
        }

        public async Task<bool> UpdateAsync(int id, string userId, bool isAdmin, UpdateArtistDto dto)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null) return false;
            if (!isAdmin && profile.UserId != userId) return false;
            profile.ArtistName = dto.ArtistName;
            profile.Bio = dto.Bio;
            profile.SocialLinksJson = dto.SocialLinksJson;
            profile.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(profile);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null) return false;
            if (!isAdmin && profile.UserId != userId) return false;
            await _repo.DeleteAsync(profile);
            return true;
        }
    }
}
