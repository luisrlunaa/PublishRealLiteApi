using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Videos;

public static class DeleteVideo
{
    public record Command(int Id, int ArtistProfileId);

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var video = await db.ArtistVideos
                .FirstOrDefaultAsync(v => v.Id == cmd.Id && v.ArtistProfileId == cmd.ArtistProfileId && !v.IsDeleted, ct);

            if (video == null) return false;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.Id == video.ArtistProfileId && !p.IsDeleted, ct);

            if (!isAdmin && profile?.UserId != userId) return false;

            db.ArtistVideos.Remove(video);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
