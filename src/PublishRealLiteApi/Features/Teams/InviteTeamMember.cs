using FluentValidation;
using Microsoft.Extensions.Configuration;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Infrastructure.Services;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Teams;

public static class InviteTeamMember
{
    public record Command(int TeamId, string Email);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TeamId).GreaterThan(0);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler(AppDbContext db, IEmailService emailService, IConfiguration configuration)
    {
        public async Task HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var invite = new TeamInvite
            {
                TeamId = cmd.TeamId,
                Email = cmd.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            db.TeamInvites.Add(invite);
            await db.SaveChangesAsync(ct);

            var appUrl = configuration["AppSettings:AppUrl"] ?? "https://localhost:3000";
            var inviteLink = $"{appUrl}/invite/{Uri.EscapeDataString(token)}";

            var subject = "You've been invited to join a team on PublishReal";
            var htmlBody = $"""
                <h1>Team Invitation</h1>
                <p>You've been invited to collaborate on PublishReal.</p>
                <p>Click the link below to accept the invitation:</p>
                <p><a href='{inviteLink}'>Accept Invitation</a></p>
                <br />
                <p>This invitation expires in 7 days. If you did not expect this, please ignore this email.</p>
                """;

            await emailService.SendEmailAsync(cmd.Email, subject, htmlBody);
        }
    }
}
