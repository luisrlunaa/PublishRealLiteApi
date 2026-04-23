using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Persistence.Repositories
{
    public class TeamInviteRepository : ITeamInviteRepository
    {
        private readonly AppDbContext _db;

        public TeamInviteRepository(AppDbContext db) => _db = db;

        public async Task<TeamInvite> AddAsync(TeamInvite invite)
        {
            _db.TeamInvites.Add(invite);
            await _db.SaveChangesAsync();
            return invite;
        }

        public async Task<TeamInvite?> GetByTokenAsync(string token)
        {
            return await _db.TeamInvites.Include(i => i.Team).FirstOrDefaultAsync(i => i.Token == token && !i.IsDeleted);
        }

        public async Task<bool> UpdateAsync(TeamInvite invite)
        {
            _db.TeamInvites.Update(invite);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
