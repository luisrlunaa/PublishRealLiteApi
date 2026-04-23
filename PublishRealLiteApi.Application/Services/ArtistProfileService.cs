using Microsoft.AspNetCore.Identity;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using Microsoft.Extensions.Configuration;

namespace PublishRealLiteApi.Application.Services
{
    public class ArtistProfileService : IArtistProfileService
    {
        private readonly IArtistProfileRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ArtistProfileService(
            IArtistProfileRepository repo,
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            IConfiguration config)
        {
            _repo = repo;
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
        }

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

        public async Task<ArtistProfileDto?> CreateAsync(string userId, CreateArtistDto dto, bool isAdmin)
        {
            var profile = new ArtistProfile
            {
                UserId = userId,
                ArtistName = dto.ArtistName,
                Bio = dto.Bio,
                SocialLinksJson = dto.SocialLinksJson,
                IsAdminProfile = isAdmin,
                CreatedBy = userId,
                AdminUserId = isAdmin ? userId : string.Empty
            };

            await _repo.AddAsync(profile);
            return new ArtistProfileDto(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson, profile.IsAdminProfile);
        }

        public async Task<ArtistProfileDto?> CreateWithAdminCodeAsync(string adminUserId, CreateArtistWithAdminCodeDto dto)
        {
            // 1. Validar que el admin existe
            var adminExists = await _repo.AdminExistsByUserIdAsync(adminUserId);
            if (!adminExists) return null;

            // 2. Crear el nuevo IdentityUser (Compatible con Application layer)
            var newUser = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user: {errors}");
            }

            try
            {
                // 3. Crear el perfil de artista vinculado al nuevo usuario
                var profile = new ArtistProfile
                {
                    UserId = newUser.Id,
                    AdminUserId = adminUserId,
                    ArtistName = dto.ArtistName,
                    Bio = dto.Bio,
                    SocialLinksJson = dto.SocialLinksJson,
                    IsAdminProfile = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = adminUserId,
                };

                await _repo.AddAsync(profile);

                // 4. Generar token de invitación (Password Reset)
                var token = await _userManager.GeneratePasswordResetTokenAsync(newUser);
                var encodedToken = Uri.EscapeDataString(token);
                var appUrl = _config["AppSettings:AppUrl"] ?? "https://localhost:3000";
                var inviteLink = $"{appUrl}/auth/set-password?userId={newUser.Id}&token={encodedToken}";

                // 5. Enviar email
                await _emailService.SendInvitationEmailAsync(
                    dto.Email,
                    dto.ArtistName,
                    inviteLink
                );

                return new ArtistProfileDto(
                    profile.Id,
                    profile.ArtistName,
                    profile.Bio,
                    profile.ProfileImageUrl,
                    profile.SocialLinksJson,
                    profile.IsAdminProfile
                );
            }
            catch (Exception)
            {
                // Rollback: Si algo falla, eliminamos el usuario creado para evitar huérfanos
                await _userManager.DeleteAsync(newUser);
                throw;
            }
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
            profile.UpdatedBy = userId;

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