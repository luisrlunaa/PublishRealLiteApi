using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Infrastructure.Services;

namespace PublishRealLiteApi.IntegrationTests.Infrastructure;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    internal const string TestConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=PublishRealLiteTest;Trusted_Connection=True;TrustServerCertificate=True;";

    public DatabaseManager Database { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Storage:Provider"] = "Local"
            }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<ITurnstileService>();
            services.RemoveAll<IEmailService>();
            services.AddSingleton<ITurnstileService, AlwaysTrueTurnstileService>();
            services.AddSingleton<IEmailService, NoOpEmailService>();
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await SeedRolesAsync(scope.ServiceProvider);

        Database = await DatabaseManager.CreateAsync(TestConnectionString);
    }

    public new async Task DisposeAsync() => await base.DisposeAsync();

    public async Task ResetDatabaseAsync()
    {
        await Database.ResetAsync();
        using var scope = Services.CreateScope();
        await SeedRolesAsync(scope.ServiceProvider);
    }

    private static async Task SeedRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["Admin", "Artist"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private sealed class AlwaysTrueTurnstileService : ITurnstileService
    {
        public Task<bool> ValidateAsync(string token) => Task.FromResult(true);
    }

    private sealed class NoOpEmailService : IEmailService
    {
        public Task SendInvitationEmailAsync(string email, string artistName, string inviteLink) => Task.CompletedTask;
        public Task SendEmailAsync(string email, string subject, string htmlMessage) => Task.CompletedTask;
    }
}
