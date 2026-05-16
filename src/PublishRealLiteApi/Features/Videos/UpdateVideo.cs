using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Videos;

public static class UpdateVideo
{
    public record Command(int Id, string Title, string ThumbnailUrl, string VideoUrl);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.VideoUrl).NotEmpty();
            RuleFor(x => x.ThumbnailUrl).NotEmpty();
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var video = await db.ArtistVideos
                .FirstOrDefaultAsync(v => v.Id == cmd.Id && !v.IsDeleted, ct);

            if (video == null) return false;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.Id == video.ArtistProfileId && !p.IsDeleted, ct);

            if (!isAdmin && profile?.UserId != userId) return false;

            video.Title = cmd.Title;
            video.ThumbnailUrl = cmd.ThumbnailUrl;
            video.VideoUrl = cmd.VideoUrl;
            video.UpdatedBy = userId;

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
