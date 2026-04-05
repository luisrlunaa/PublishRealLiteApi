using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamInviteRepository? _teamInviteRepository; // optional, if you implement invites

        public TeamService(ITeamRepository teamRepository, ITeamInviteRepository? teamInviteRepository = null)
        {
            _teamRepository = teamRepository;
            _teamInviteRepository = teamInviteRepository;
        }

        public async Task<TeamDto> CreateTeamAsync(int artistProfileId, string name)
        {
            var team = new Team { ArtistProfileId = artistProfileId, Name = name };
            var created = await _teamRepository.AddAsync(team);
            return new TeamDto { Id = created.Id, Name = created.Name };
        }

        public async Task InviteAsync(int teamId, string email)
        {
            if (_teamInviteRepository == null) throw new InvalidOperationException("Invite repository not configured.");
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var invite = new TeamInvite { TeamId = teamId, Email = email, Token = token, ExpiresAt = DateTime.UtcNow.AddDays(7) };
            await _teamInviteRepository.AddAsync(invite);
            // envío de email queda en capa API o servicio de infraestructura
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsForArtistAsync(int artistProfileId)
        {
            var teams = await _teamRepository.GetByArtistAsync(artistProfileId);
            return teams.Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Members = t.Members?.Select(m => new TeamMemberDto { Id = m.Id, Email = m.Email, Role = m.Role, SharePercent = m.SharePercent, Accepted = m.Accepted }).ToList()
            });
        }

        public async Task AcceptInviteAsync(string token, string userEmail)
        {
            if (_teamInviteRepository == null) throw new InvalidOperationException("Invite repository not configured.");
            var inv = await _teamInviteRepository.GetByTokenAsync(token);
            if (inv == null || inv.Used || inv.ExpiresAt <= DateTime.UtcNow) throw new InvalidOperationException("Invalid invite");
            inv.Used = true;
            await _teamInviteRepository.UpdateAsync(inv);
            var member = new TeamMember { TeamId = inv.TeamId, Email = userEmail, Role = "Editor", Accepted = true };
            await _teamRepository.AddMemberAsync(member);
        }
    }
}
