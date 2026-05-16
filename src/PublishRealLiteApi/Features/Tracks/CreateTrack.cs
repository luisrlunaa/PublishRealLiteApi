using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Tracks;

public static class CreateTrack
{
    public record Command(Guid ReleaseId, int Position, string Title);

    public record Response(Guid Id, Guid ReleaseId, int Position, string Title);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ReleaseId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Position).GreaterThan(0);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var release = await db.Releases
                .Include(r => r.ArtistProfile)
                .FirstOrDefaultAsync(r => r.Id == cmd.ReleaseId && !r.IsDeleted, ct);

            if (release == null) return null;

            var authorized = isAdmin || release.ArtistProfile?.UserId == userId;
            if (!authorized) return null;

            var track = new Track
            {
                ReleaseId = cmd.ReleaseId,
                Position = cmd.Position,
                Title = cmd.Title,
                CreatedBy = userId
            };

            db.Tracks.Add(track);
            await db.SaveChangesAsync(ct);

            return new Response(track.Id, track.ReleaseId, track.Position, track.Title);
        }
    }
}
