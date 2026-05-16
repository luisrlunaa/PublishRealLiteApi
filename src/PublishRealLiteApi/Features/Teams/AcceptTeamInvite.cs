using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Teams;

public static class AcceptTeamInvite
{
    public record Command(string Token, string UserEmail);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
        }
    }

    public class Handler(AppDbContext db)
    {
        public async Task HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var invite = await db.TeamInvites
                .FirstOrDefaultAsync(i => i.Token == cmd.Token && !i.IsDeleted, ct);

            if (invite == null || invite.Used || invite.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Invalid invite");

            invite.Used = true;

            var member = new TeamMember
            {
                TeamId = invite.TeamId,
                Email = cmd.UserEmail,
                Role = "Editor",
                Accepted = true
            };

            db.TeamMembers.Add(member);
            await db.SaveChangesAsync(ct);
        }
    }
}
