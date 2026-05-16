using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Videos;

public static class CreateVideo
{
    public record Command(int ArtistProfileId, string Title, string ThumbnailUrl, string VideoUrl);

    public record Response(int Id, int ArtistProfileId, string Title, string ThumbnailUrl, string VideoUrl);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistProfileId).GreaterThan(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.VideoUrl).NotEmpty();
            RuleFor(x => x.ThumbnailUrl).NotEmpty();
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.Id == cmd.ArtistProfileId && !p.IsDeleted, ct);

            if (profile == null) return null;
            if (!isAdmin && profile.UserId != userId) return null;

            var video = new ArtistVideo
            {
                ArtistProfileId = cmd.ArtistProfileId,
                Title = cmd.Title,
                ThumbnailUrl = cmd.ThumbnailUrl,
                VideoUrl = cmd.VideoUrl,
                CreatedBy = userId
            };

            db.ArtistVideos.Add(video);
            await db.SaveChangesAsync(ct);

            return new Response(video.Id, video.ArtistProfileId, video.Title, video.ThumbnailUrl, video.VideoUrl);
        }
    }
}
