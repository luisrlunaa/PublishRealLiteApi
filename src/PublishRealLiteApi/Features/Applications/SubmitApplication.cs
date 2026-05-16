using FluentValidation;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.Applications;

public static class SubmitApplication
{
    public record Command(
        string ArtistName,
        string Email,
        string Country,
        string InstagramUrl,
        string Role,
        string? SongAsComposerUrl,
        string? SongAsArtistUrl,
        bool AffiliatedWithPro,
        string OwnershipType,
        bool InterestedInSigning,
        string TurnstileToken);

    public record Response(int Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistName).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Country).NotEmpty();
            RuleFor(x => x.InstagramUrl).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
            RuleFor(x => x.OwnershipType).NotEmpty();
            RuleFor(x => x.TurnstileToken).NotEmpty();
        }
    }

    public class Handler(AppDbContext db, ITurnstileService turnstile)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            if (!await turnstile.ValidateAsync(cmd.TurnstileToken))
                return null;

            var application = new ArtistApplication
            {
                ArtistName = cmd.ArtistName,
                Email = cmd.Email,
                Country = cmd.Country,
                InstagramUrl = cmd.InstagramUrl,
                Role = cmd.Role,
                SongAsComposerUrl = cmd.SongAsComposerUrl,
                SongAsArtistUrl = cmd.SongAsArtistUrl,
                AffiliatedWithPro = cmd.AffiliatedWithPro,
                OwnershipType = cmd.OwnershipType,
                InterestedInSigning = cmd.InterestedInSigning,
                CreatedAt = DateTime.UtcNow
            };

            db.ArtistApplications.Add(application);
            await db.SaveChangesAsync(ct);

            return new Response(application.Id);
        }
    }
}
