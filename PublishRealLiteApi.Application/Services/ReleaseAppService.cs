using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Application.Services
{
    public class ReleaseAppService : IReleaseService
    {
        private readonly IReleaseRepository _repo;
        public ReleaseAppService(IReleaseRepository repo) => _repo = repo;

        public async Task<IEnumerable<ReleaseDto>> GetAllAsync()
        {
            var list = await _repo.GetByArtistAsync(0); // default: return all via repo implementation
            return list.Select(r => new ReleaseDto(r.Id, r.ArtistProfileId, r.Title, r.ReleaseDate, r.Genre, r.Label, r.UPC, r.ISRC, r.LinksJson));
        }

        public async Task<ReleaseDto?> GetByIdAsync(Guid id)
        {
            var r = await _repo.GetByIdAsync(id);
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
            var added = await _repo.AddAsync(release);
            return new ReleaseDto(added.Id, added.ArtistProfileId, added.Title, added.ReleaseDate, added.Genre, added.Label, added.UPC, added.ISRC, added.LinksJson);
        }

        public async Task<bool> UpdateAsync(Guid id, string userId, bool isAdmin, UpdateReleaseDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            existing.Title = dto.Title;
            existing.ReleaseDate = dto.ReleaseDate;
            existing.Genre = dto.Genre;
            existing.Label = dto.Label;
            existing.UPC = dto.UPC;
            existing.ISRC = dto.ISRC;
            existing.LinksJson = dto.LinksJson;
            return await _repo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId, bool isAdmin)
        {
            return await _repo.DeleteAsync(id, 0);
        }
    }
}
