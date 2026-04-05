using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly IReleaseRepository _releaseRepository;

        public ReleaseService(IReleaseRepository releaseRepository)
        {
            _releaseRepository = releaseRepository;
        }

        public async Task<IEnumerable<Release>> GetReleasesAsync(int artistProfileId)
        {
            return await _releaseRepository.GetByArtistAsync(artistProfileId);
        }

        public async Task<Release?> GetReleaseByIdAsync(Guid id, int artistProfileId)
        {
            // Repositorio puede usar Guid o int id según tu modelo; aquí asumimos Guid for Release.Id
            return await _releaseRepository.GetByIdAsync(id, artistProfileId);
        }

        public async Task<Release> CreateReleaseAsync(Release release)
        {
            return await _releaseRepository.AddAsync(release);
        }

        public async Task<bool> UpdateReleaseAsync(Release release)
        {
            return await _releaseRepository.UpdateAsync(release);
        }

        public async Task<bool> DeleteReleaseAsync(Guid id, int artistProfileId)
        {
            return await _releaseRepository.DeleteAsync(id, artistProfileId);
        }
    }
}
