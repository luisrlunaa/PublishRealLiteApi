using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Features.Auth;

public static class Login
{
    public record Command(string Email, string Password, string TurnstileToken);

    public record Response(string Token, string? Email);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.TurnstileToken).NotEmpty();
        }
    }

    public class Handler(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        ITurnstileService turnstile)
    {
        public async Task<Response?> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            if (!await turnstile.ValidateAsync(cmd.TurnstileToken))
                return null;

            var user = await userManager.FindByEmailAsync(cmd.Email);
            if (user == null) return null;

            var result = await signInManager.CheckPasswordSignInAsync(user, cmd.Password, false);
            if (!result.Succeeded) return null;

            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);
            return new Response(token, user.Email);
        }
    }
}
