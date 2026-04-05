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

namespace PublishRealLiteApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Identity: AddIdentityCore + AddRoles + AddEntityFrameworkStores para registrar stores y RoleManager correctamente
        services.AddIdentityCore<AppUser>(options =>
        {
            // Ajusta opciones si lo necesitas
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Application services
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IVideoService, VideoService>();

        // Repositories
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<IStatsRepository, StatsRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamInviteRepository, TeamInviteRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();

        // Health checks
        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }
}
