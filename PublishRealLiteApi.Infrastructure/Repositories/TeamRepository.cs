using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Persistence.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _db;

        public TeamRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Team>> GetByArtistAsync(int artistProfileId)
        {
            return await _db.Teams
                .Where(t => t.ArtistProfileId == artistProfileId && !t.IsDeleted)
                .Include(t => t.Members)
                .ToListAsync();
        }

        public async Task<Team?> GetByIdAsync(int id, int artistProfileId)
        {
            return await _db.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id && t.ArtistProfileId == artistProfileId && !t.IsDeleted);
        }

        public async Task<Team> AddAsync(Team team)
        {
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
            return team;
        }

        public async Task<bool> UpdateAsync(Team team)
        {
            _db.Teams.Update(team);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, int artistProfileId)
        {
            var entity = await GetByIdAsync(id, artistProfileId);
            if (entity == null) return false;
            _db.Teams.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task AddMemberAsync(TeamMember member)
        {
            _db.TeamMembers.Add(member);
            await _db.SaveChangesAsync();
        }
    }
}
