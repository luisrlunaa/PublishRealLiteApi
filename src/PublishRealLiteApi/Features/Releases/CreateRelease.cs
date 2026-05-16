using FluentValidation;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Releases;

public static class CreateRelease
{
    public record Command(int ArtistProfileId, string Title, DateTime? ReleaseDate, string? Genre, string? Label, string? UPC, string? ISRC, string? LinksJson);

    public record Response(Guid Id, int ArtistProfileId, string Title);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ArtistProfileId).GreaterThan(0);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var release = new Release
            {
                ArtistProfileId = cmd.ArtistProfileId,
                Title = cmd.Title,
                ReleaseDate = cmd.ReleaseDate,
                Genre = cmd.Genre,
                Label = cmd.Label,
                UPC = cmd.UPC,
                ISRC = cmd.ISRC,
                LinksJson = cmd.LinksJson,
                CreatedBy = currentUser.UserId
            };

            db.Releases.Add(release);
            await db.SaveChangesAsync(ct);

            return new Response(release.Id, release.ArtistProfileId, release.Title);
        }
    }
}
