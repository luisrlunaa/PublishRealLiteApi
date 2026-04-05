using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Services
{
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _db;
        private readonly IEmailSender _email; // si tienes
        public TeamService(AppDbContext db, IEmailSender email = null) { _db = db; _email = email; }

        public async Task<TeamDto> CreateTeamAsync(int artistProfileId, string name)
        {
            var team = new Team { ArtistProfileId = artistProfileId, Name = name };
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();
            return new TeamDto { Id = team.Id, Name = team.Name };
        }

        public async Task InviteAsync(int teamId, string email)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var invite = new TeamInvite { TeamId = teamId, Email = email, Token = token, ExpiresAt = DateTime.UtcNow.AddDays(7) };
            _db.TeamInvites.Add(invite);
            await _db.SaveChangesAsync();
            // enviar email con link /teams/accept?token=...
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsForArtistAsync(int artistProfileId)
        {
            var teams = await _db.Teams.Include(t => t.Members).Where(t => t.ArtistProfileId == artistProfileId).ToListAsync();
            return teams.Select(t => new TeamDto { Id = t.Id, Name = t.Name, Members = t.Members.Select(m => new TeamMemberDto { Id = m.Id, Email = m.Email, Role = m.Role, SharePercent = m.SharePercent, Accepted = m.Accepted }) });
        }

        public async Task AcceptInviteAsync(string token, string userEmail)
        {
            var inv = await _db.TeamInvites.Include(i => i.Team).FirstOrDefaultAsync(i => i.Token == token && !i.Used && i.ExpiresAt > DateTime.UtcNow);
            if (inv == null) throw new InvalidOperationException("Invalid invite");
            inv.Used = true;
            var member = new TeamMember { TeamId = inv.TeamId, Email = userEmail, Role = "Editor", Accepted = true };
            _db.TeamMembers.Add(member);
            await _db.SaveChangesAsync();
        }
    }

}
