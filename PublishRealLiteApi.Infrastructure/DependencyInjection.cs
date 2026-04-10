using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Infrastructure.Identity;
using PublishRealLiteApi.Infrastructure.Persistence.Repositories;
using PublishRealLiteApi.Infrastructure.Repositories;

namespace PublishRealLiteApi.Infrastructure;

/// <summary>
/// Extension method for registering infrastructure-layer services.
/// 
/// IMPORTANT: ICurrentUserService must be registered in Program.cs BEFORE calling this method.
/// This ensures that:
/// 1. The Application layer (e.g., AuthService) can depend on ICurrentUserService
/// 2. There's a single clear point of registration in Program.cs
/// 3. Dependency direction flows from Infrastructure → Application → Core
/// 
/// If you need to move ICurrentUserService registration here, ensure it's done with AddScoped,
/// not TryAddScoped, and that it's registered before Infrastructure services that depend on it.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentityCore<AppUser>(options =>
        {
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Application services
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<IArtistProfileService, ArtistProfileService>();
        services.AddScoped<IArtistVideoService, ArtistVideoService>();
        services.AddScoped<IAuthService, AuthService>();

        // Repositories
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<IStatsRepository, StatsRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamInviteRepository, TeamInviteRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IArtistProfileRepository, ArtistProfileRepository>();
        services.AddScoped<IArtistVideoRepository, ArtistVideoRepository>();

        // Health checks
        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }
}