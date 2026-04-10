using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<ReleaseDto>> GetAllAsync()
        {
            var list = await _releaseRepository.GetByArtistAsync(0);
            return list.Select(r => new ReleaseDto(r.Id, r.ArtistProfileId, r.Title, r.ReleaseDate, r.Genre, r.Label, r.UPC, r.ISRC, r.LinksJson));
        }

        public async Task<ReleaseDto?> GetByIdAsync(Guid id)
        {
            var r = await _releaseRepository.GetByIdAsync(id);
            return r == null ? null : new ReleaseDto(r.Id, r.ArtistProfileId, r.Title, r.ReleaseDate, r.Genre, r.Label, r.UPC, r.ISRC, r.LinksJson);
        }

        public async Task<ReleaseDto?> CreateAsync(string userId, bool isAdmin, CreateReleaseDto dto)
        {
            var release = new Models.Release
            {
                ArtistProfileId = dto.ArtistProfileId,
                Title = dto.Title,
                ReleaseDate = dto.ReleaseDate,
                Genre = dto.Genre,
                Label = dto.Label,
                UPC = dto.UPC,
                ISRC = dto.ISRC,
                LinksJson = dto.LinksJson
            };
            var added = await _releaseRepository.AddAsync(release);
            return new ReleaseDto(added.Id, added.ArtistProfileId, added.Title, added.ReleaseDate, added.Genre, added.Label, added.UPC, added.ISRC, added.LinksJson);
        }

        public async Task<bool> UpdateAsync(Guid id, string userId, bool isAdmin, UpdateReleaseDto dto)
        {
            var existing = await _releaseRepository.GetByIdAsync(id);
            if (existing == null) return false;
            existing.Title = dto.Title;
            existing.ReleaseDate = dto.ReleaseDate;
            existing.Genre = dto.Genre;
            existing.Label = dto.Label;
            existing.UPC = dto.UPC;
            existing.ISRC = dto.ISRC;
            existing.LinksJson = dto.LinksJson;
            return await _releaseRepository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId, bool isAdmin)
        {
            return await _releaseRepository.DeleteAsync(id, 0);
        }
    }
}
