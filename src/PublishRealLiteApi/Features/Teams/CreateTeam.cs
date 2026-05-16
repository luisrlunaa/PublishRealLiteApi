using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Teams;

public static class CreateTeam
{
    public record Command(int ArtistProfileId, string Name);

    public record Response(int Id, string Name);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistProfileId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var team = new Team
            {
                ArtistProfileId = cmd.ArtistProfileId,
                Name = cmd.Name,
                CreatedBy = currentUser.UserId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync(ct);

            return new Response(team.Id, team.Name);
        }
    }
}
