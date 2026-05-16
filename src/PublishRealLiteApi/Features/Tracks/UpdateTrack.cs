using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Tracks;

public static class UpdateTrack
{
    public record Command(Guid Id, int Position, string Title);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Position).GreaterThan(0);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var track = await db.Tracks
                .FirstOrDefaultAsync(t => t.Id == cmd.Id && !t.IsDeleted, ct);

            if (track == null) return false;

            var release = await db.Releases
                .Include(r => r.ArtistProfile)
                .FirstOrDefaultAsync(r => r.Id == track.ReleaseId && !r.IsDeleted, ct);

            if (release == null) return false;

            var authorized = isAdmin || release.ArtistProfile?.UserId == userId;
            if (!authorized) return false;

            track.Position = cmd.Position;
            track.Title = cmd.Title;
            track.UpdatedBy = userId;

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
