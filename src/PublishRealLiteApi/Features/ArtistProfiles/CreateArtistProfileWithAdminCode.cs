using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Infrastructure.Services;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Features.ArtistProfiles;

public static class CreateArtistProfileWithAdminCode
{
    public record Command(string ArtistName, string Email, string? Bio, string? SocialLinksJson);

    public record Response(int Id, string ArtistName, string? Bio, string? ProfileImageUrl, string? SocialLinksJson, bool IsAdminProfile);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ArtistName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler(
        AppDbContext db,
        ICurrentUserService currentUser,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        IEmailService emailService)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var adminUserId = currentUser.UserId!;

            var adminExists = await db.ArtistProfiles
                .AnyAsync(p => p.UserId == adminUserId && p.IsAdminProfile && !p.IsDeleted, ct);

            if (!adminExists) return null;

            var newUser = new IdentityUser
            {
                UserName = cmd.Email,
                Email = cmd.Email,
                EmailConfirmed = false
            };

            var result = await userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            try
            {
                var profile = new ArtistProfile
                {
                    UserId = newUser.Id,
                    AdminUserId = adminUserId,
                    ArtistName = cmd.ArtistName,
                    Bio = cmd.Bio,
                    SocialLinksJson = cmd.SocialLinksJson,
                    IsAdminProfile = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = adminUserId
                };

                db.ArtistProfiles.Add(profile);
                await db.SaveChangesAsync(ct);

                var token = await userManager.GeneratePasswordResetTokenAsync(newUser);
                var encodedToken = Uri.EscapeDataString(token);
                var appUrl = configuration["AppSettings:AppUrl"] ?? "https://localhost:3000";
                var inviteLink = $"{appUrl}/auth/set-password?userId={newUser.Id}&token={encodedToken}";

                await emailService.SendInvitationEmailAsync(cmd.Email, cmd.ArtistName, inviteLink);

                return new Response(profile.Id, profile.ArtistName, profile.Bio, profile.ProfileImageUrl, profile.SocialLinksJson, profile.IsAdminProfile);
            }
            catch
            {
                await userManager.DeleteAsync(newUser);
                throw;
            }
        }
    }
}
