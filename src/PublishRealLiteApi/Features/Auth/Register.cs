using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Features.Auth;

public static class Register
{
    public record Command(string Email, string Password, string TurnstileToken);

    public record Response(string Id, string? Email);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.TurnstileToken).NotEmpty();
        }
    }

    public class Handler(
        UserManager<IdentityUser> userManager,
        ITurnstileService turnstile)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            if (!await turnstile.ValidateAsync(cmd.TurnstileToken))
                return null;

            var user = new IdentityUser { UserName = cmd.Email, Email = cmd.Email };
            var result = await userManager.CreateAsync(user, cmd.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, "Artist");
            return new Response(user.Id, user.Email);
        }
    }
}
