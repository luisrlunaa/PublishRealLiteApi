using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Releases;

public static class UpdateRelease
{
    public record Command(Guid Id, string Title, DateTime? ReleaseDate, string? Genre, string? Label, string? UPC, string? ISRC, string? LinksJson);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<bool> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var existing = await db.Releases
                .FirstOrDefaultAsync(r => r.Id == cmd.Id && !r.IsDeleted, ct);

            if (existing == null) return false;

            existing.Title = cmd.Title;
            existing.ReleaseDate = cmd.ReleaseDate;
            existing.Genre = cmd.Genre;
            existing.Label = cmd.Label;
            existing.UPC = cmd.UPC;
            existing.ISRC = cmd.ISRC;
            existing.LinksJson = cmd.LinksJson;
            existing.UpdatedBy = currentUser.UserId;

            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
