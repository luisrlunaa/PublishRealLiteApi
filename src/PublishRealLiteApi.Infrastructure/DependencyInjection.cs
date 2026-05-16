using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Infrastructure.Services;
using Resend;

namespace PublishRealLiteApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddResend(options =>
        {
            options.ApiToken = config["Resend:ApiKey"] ?? string.Empty;
        });
        services.AddScoped<IEmailService, ResendEmailService>();

        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }
}
