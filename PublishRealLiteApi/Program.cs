using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Application;
using PublishRealLiteApi.Application.Services.Interfaces;
using PublishRealLiteApi.Infrastructure;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Services;
using PublishRealLiteApi.Services.Interfaces;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly);
    cfg.AddMaps(typeof(IApplicationMarker).Assembly);
});

// Register ICurrentUserService from Application layer
// This must be registered before AddInfrastructure to establish the contract
// that Application-layer services can depend on
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Infrastructure (DbContext, Identity, repos, health checks, IAuthService, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// App services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUploadService, LocalUploadService>();

builder.Services.AddSingleton<SimpleRateLimitMiddleware>();

var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "Local";
if (storageProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    builder.Services.AddScoped<IStorageService, LocalStorageService>();

// JWT Authentication
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
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://your-frontend-domain.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddHostedService<PublishRealLiteApi.Workers.StreamStatAggregatorWorker>();
builder.Services.AddScoped<IStreamStatAggregatorService, PublishRealLiteApi.Services.StreamStatAggregatorService>();

var app = builder.Build();

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
        var asmList = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.FullName != null &&
                        a.FullName.StartsWith("PublishRealLiteApi", StringComparison.OrdinalIgnoreCase))
            .ToList();

        logger.LogInformation("Scanning {Count} assemblies for IEntityTypeConfiguration<> implementations...", asmList.Count);

        var found = new List<string>();
        foreach (var asm in asmList)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException rtle)
            { types = rtle.Types.Where(t => t != null).ToArray()!; }

            foreach (var t in types.Where(t => t is { IsAbstract: false, IsInterface: false }))
            {
                var ifaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
                    .ToArray();
                if (ifaces.Length == 0) continue;

                foreach (var iface in ifaces)
                {
                    var ent = iface.GetGenericArguments()[0];
                    var entry = $"{asm.GetName().Name}: {t.FullName} -> IEntityTypeConfiguration<{ent.FullName}>";
                    found.Add(entry);
                    logger.LogInformation(entry);
                }
            }
        }

        if (!found.Any())
            logger.LogInformation("No IEntityTypeConfiguration<> implementations found.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error scanning assemblies for IEntityTypeConfiguration<> implementations.");
    }

    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        var allMigrations = db.Database.GetMigrations().ToList();
        if (!allMigrations.Any())
        {
            if (!db.Database.CanConnect())
            {
                logger.LogInformation("No migrations and DB unreachable — creating schema with EnsureCreated()...");
                db.Database.EnsureCreated();
            }
            else
            {
                var applied = db.Database.GetAppliedMigrations().ToList();
                if (!applied.Any())
                {
                    logger.LogInformation("No migrations or applied history — creating schema with EnsureCreated()...");
                    db.Database.EnsureCreated();
                }
                else
                {
                    logger.LogInformation("No migrations in project but DB already has schema. Skipping.");
                }
            }
        }
        else
        {
            if (!db.Database.CanConnect())
            {
                logger.LogInformation("DB unreachable — creating and migrating...");
                db.Database.Migrate();
            }
            else
            {
                var applied = db.Database.GetAppliedMigrations().ToList();

                if (!applied.Any())
                {
                    bool dbHasTables = false;
                    try
                    {
                        var conn = db.Database.GetDbConnection();
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES";
                        dbHasTables = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not determine whether DB already has tables.");
                    }

                    if (dbHasTables)
                    {
                        logger.LogWarning("DB has existing schema but no applied EF migrations. Skipping Migrate() to avoid conflicts.");
                        logger.LogWarning("Action: create a baseline migration, mark it applied, or run 'dotnet ef database update' in CI/CD.");
                    }
                    else
                    {
                        var pending = db.Database.GetPendingMigrations().ToList();
                        if (pending.Any())
                        {
                            logger.LogInformation("Applying {Count} pending migration(s)...", pending.Count);
                            db.Database.Migrate();
                        }
                        else
                        {
                            logger.LogInformation("No pending migrations. DB is up-to-date.");
                        }
                    }
                }
                else
                {
                    var pending = db.Database.GetPendingMigrations().ToList();
                    if (pending.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migration(s)...", pending.Count);
                        db.Database.Migrate();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations. DB is up-to-date.");
                    }
                }
            }
        }

        await DbSeeder.SeedAsync(services);
    }
    catch (InvalidOperationException invEx) when (
        invEx.Message?.Contains("resolved to System.Object") == true ||
        invEx.Message?.Contains("entity type 'object'") == true ||
        invEx.Message?.Contains("requires a primary key") == true)
    {
        logger.LogError(invEx, "Startup migration failed — possible 'object' entity mapping in EF model.");
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup migration/seeding error.");
        throw;
    }
}

app.Run();