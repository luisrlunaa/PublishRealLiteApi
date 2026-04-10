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
        services.AddScoped<ICurrentUserService, CurrentUserService>(); 
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