using PublishRealLiteApi.Application.DTOs;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Services
{
    public class ArtistApplicationService : IArtistApplicationService
    {
        private readonly IArtistApplicationRepository _repo;
        private readonly ITurnstileService _turnstile;

        public ArtistApplicationService(IArtistApplicationRepository repo, ITurnstileService turnstile)
        {
            _repo = repo;
            _turnstile = turnstile;
        }

        public async Task<bool> SubmitAsync(SubmitApplicationDto dto)
        {
            if (!await _turnstile.ValidateAsync(dto.TurnstileToken))
                return false;

            var application = new ArtistApplication
            {
                ArtistName = dto.ArtistName,
                Email = dto.Email,
                Country = dto.Country,
                InstagramUrl = dto.InstagramUrl,
                Role = dto.Role,
                SongAsComposerUrl = dto.SongAsComposerUrl,
                SongAsArtistUrl = dto.SongAsArtistUrl,
                AffiliatedWithPro = dto.AffiliatedWithPro,
                OwnershipType = dto.OwnershipType,
                InterestedInSigning = dto.InterestedInSigning,
            };

            await _repo.AddAsync(application);
            return true;
        }
    }
}
