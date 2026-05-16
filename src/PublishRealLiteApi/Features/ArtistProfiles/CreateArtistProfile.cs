using FluentValidation;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class CreateArtistProfile
{
    public record Command(string ArtistName, string? Bio, string? SocialLinksJson);

    public record Response(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistName).NotEmpty().MaximumLength(200);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;
            var isAdmin = currentUser.IsAdmin;

            var profile = new ArtistProfile
            {
                UserId = userId,
                ArtistName = cmd.ArtistName,
                Bio = cmd.Bio,
                SocialLinksJson = cmd.SocialLinksJson,
                IsAdminProfile = isAdmin,
                CreatedBy = userId,
                AdminUserId = isAdmin ? userId : string.Empty
            };

            db.ArtistProfiles.Add(profile);
            await db.SaveChangesAsync(ct);

            return new Response(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson, profile.IsAdminProfile);
        }
    }
}
