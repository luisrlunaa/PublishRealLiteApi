using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Features.Stats.Workers;
using PublishRealLiteApi.Infrastructure;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Services;
using PublishRealLiteApi.Services.Interfaces;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cross-cutting services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUploadService, LocalUploadService>();
builder.Services.AddSingleton<SimpleRateLimitMiddleware>();
builder.Services.AddHttpClient<ITurnstileService, TurnstileService>();

var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "Local";
if (storageProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    builder.Services.AddScoped<IStorageService, LocalStorageService>();

// Feature handlers (auto-registered via Scrutor)
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(c => c.Where(t => t.Name == "Handler"))
    .AsSelf()
    .WithScopedLifetime());

// FluentValidation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<Program>();

// Stats worker
builder.Services.AddHostedService<StreamStatAggregatorWorker>();
builder.Services.AddScoped<IStreamStatAggregatorService, StreamStatAggregatorService>();

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSection.GetValue<string>("Secret")
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
var issuer = jwtSection.GetValue<string>("Issuer") ?? "PublishRealLite";
var audience = jwtSection.GetValue<string>("Audience") ?? "PublishRealLiteUsers";

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SimpleRateLimitMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "PublishRealLite API";
    options.Theme = ScalarTheme.DeepSpace;
});

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1");
    return Task.CompletedTask;
});

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.MapControllers();

app.MapPost("/admin/aggregate-stats", async (IStreamStatAggregatorService svc, CancellationToken ct) =>
{
    await svc.AggregateOnceAsync(ct);
    return Results.Ok(new { message = "Aggregation executed" });
}).RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var allMigrations = db.Database.GetMigrations().ToList();
        if (!allMigrations.Any())
        {
            if (!db.Database.CanConnect() || !db.Database.GetAppliedMigrations().Any())
                db.Database.EnsureCreated();
        }
        else
        {
            var pending = db.Database.GetPendingMigrations().ToList();
            if (pending.Any())
                db.Database.Migrate();
            else
                logger.LogInformation("No pending migrations. DB is up-to-date.");
        }

        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "CRITICAL: Startup migration/seeding error. Check your SQL Server connection.");
        throw;
    }
}

app.Run();

public partial class Program { }
