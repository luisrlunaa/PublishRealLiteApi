using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class DeleteArtistProfile
{
    public record Command(int Id);

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

            db.ArtistProfiles.Remove(profile);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
