using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class UpdateArtistProfile
{
    public record Command(int Id, string ArtistName, string? Bio, string? SocialLinksJson);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistName).NotEmpty().MaximumLength(200);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.Id == cmd.Id && !p.IsDeleted, ct);

            if (profile == null) return false;
            if (!isAdmin && profile.UserId != userId) return false;

            profile.ArtistName = cmd.ArtistName;
            profile.Bio = cmd.Bio;
            profile.SocialLinksJson = cmd.SocialLinksJson;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
