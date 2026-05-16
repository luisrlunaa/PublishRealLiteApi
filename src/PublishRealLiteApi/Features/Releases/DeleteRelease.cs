using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Releases;

public static class DeleteRelease
{
    public record Command(Guid Id, int ArtistProfileId);

    public class Handler(AppDbContext db)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var release = await db.Releases
                .FirstOrDefaultAsync(r => r.Id == cmd.Id && r.ArtistProfileId == cmd.ArtistProfileId && !r.IsDeleted, ct);

            if (release == null) return false;

            db.Releases.Remove(release);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
