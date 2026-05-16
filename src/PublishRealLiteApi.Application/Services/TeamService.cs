using Microsoft.Extensions.Configuration;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamInviteRepository _teamInviteRepository;
        private readonly IEmailService _emailService;
        private readonly string _appUrl;

        public TeamService(
            ITeamRepository teamRepository,
            ITeamInviteRepository teamInviteRepository,
            IEmailService emailService,
            IConfiguration config)
        {
            _teamRepository = teamRepository;
            _teamInviteRepository = teamInviteRepository;
            _emailService = emailService;
            _appUrl = config["AppSettings:AppUrl"] ?? "https://localhost:3000";
        }

        public async Task<TeamDto> CreateTeamAsync(int artistProfileId, string name)
        {
            var team = new Team { ArtistProfileId = artistProfileId, Name = name };
            var created = await _teamRepository.AddAsync(team);
            return new TeamDto { Id = created.Id, Name = created.Name };
        }

        public async Task InviteAsync(int teamId, string email)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var invite = new TeamInvite { TeamId = teamId, Email = email, Token = token, ExpiresAt = DateTime.UtcNow.AddDays(7) };
            await _teamInviteRepository.AddAsync(invite);

            var inviteLink = $"{_appUrl}/invite/{Uri.EscapeDataString(token)}";
            var subject = "You've been invited to join a team on PublishReal";
            var htmlBody = $"""
                <h1>Team Invitation</h1>
                <p>You've been invited to collaborate on PublishReal.</p>
                <p>Click the link below to accept the invitation:</p>
                <p><a href='{inviteLink}'>Accept Invitation</a></p>
                <br />
                <p>This invitation expires in 7 days. If you did not expect this, please ignore this email.</p>
                """;

            await _emailService.SendEmailAsync(email, subject, htmlBody);
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
            var inv = await _teamInviteRepository.GetByTokenAsync(token);
            if (inv == null || inv.Used || inv.ExpiresAt <= DateTime.UtcNow) throw new InvalidOperationException("Invalid invite");
            inv.Used = true;
            await _teamInviteRepository.UpdateAsync(inv);
            var member = new TeamMember { TeamId = inv.TeamId, Email = userEmail, Role = "Editor", Accepted = true };
            await _teamRepository.AddMemberAsync(member);
        }
    }
}
