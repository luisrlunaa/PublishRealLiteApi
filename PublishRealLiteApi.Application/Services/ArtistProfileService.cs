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
            return list.ConvertAll(p => new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile));
        }

        public async Task<ArtistProfileDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p == null ? null : new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile);
        }

        public async Task<ArtistProfileDto?> CreateAsync(string userId, CreateArtistDto dto)
        {
            if (await _repo.ExistsForUserAsync(userId)) return null;
            var profile = new ArtistProfile
            {
                UserId = userId,
                ArtistName = dto.ArtistName,
                Bio = dto.Bio,
                SocialLinksJson = dto.SocialLinksJson,
                IsAdminProfile = true
            };
            await _repo.AddAsync(profile);
            return new ArtistProfileDto(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson, profile.IsAdminProfile);
        }

        public async Task<ArtistProfileDto?> CreateWithAdminCodeAsync(string userId, CreateArtistWithAdminCodeDto dto)
        {
            if (await _repo.ExistsForUserAsync(userId)) return null;

            // Validate that the adminUserId exists and is an admin
            if (!string.IsNullOrEmpty(dto.AdminUserId))
            {
                var adminExists = await _repo.AdminExistsByUserIdAsync(dto.AdminUserId);
                if (!adminExists) return null;
            }

            var profile = new ArtistProfile
            {
                UserId = userId,
                AdminUserId = dto.AdminUserId,
                ArtistName = dto.ArtistName,
                Bio = dto.Bio,
                SocialLinksJson = dto.SocialLinksJson,
                IsAdminProfile = string.IsNullOrEmpty(dto.AdminUserId)
            };

            await _repo.AddAsync(profile);
            return new ArtistProfileDto(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson, profile.IsAdminProfile);
        }

        public async Task<AdminProfileResponseDto?> GetAdminProfileWithSubProfilesAsync(string userId)
        {
            var profile = await _repo.GetAdminProfileByUserIdAsync(userId);
            if (profile == null) return null;

            var subProfiles = await _repo.GetSubProfilesByAdminAsync(userId);

            return new AdminProfileResponseDto(
                profile.Id,
                profile.UserId,
                profile.ArtistName,
                profile.Bio,
                profile.ProfileImageUrl,
                profile.SocialLinksJson,
                profile.UserId,
                subProfiles.ConvertAll(p => new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile))
            );
        }

        public async Task<List<ArtistProfileDto>> GetSubProfilesAsync(string adminUserId)
        {
            var profiles = await _repo.GetSubProfilesByAdminAsync(adminUserId);
            return profiles.ConvertAll(p => new ArtistProfileDto(p.Id, p.ArtistName, p.Bio, p.ProfileImageUrl, p.SocialLinksJson, p.IsAdminProfile));
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
