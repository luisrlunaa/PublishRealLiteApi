using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Teams;

public static class CreateTeam
{
    public record Command(string Name);

    public record Response(int Id, string Name);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);

            if (profile == null) return null;

            var team = new Team
            {
                ArtistProfileId = profile.Id,
                Name = cmd.Name,
                CreatedBy = userId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync(ct);

            return new Response(team.Id, team.Name);
        }
    }
}
