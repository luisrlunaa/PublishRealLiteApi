using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Infrastructure;
using PublishRealLiteApi.Infrastructure.Data;
using PublishRealLiteApi.Services;
using PublishRealLiteApi.Services.Interfaces;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Infrastructure (DbContext, Identity, repos, health checks)
builder.Services.AddInfrastructure(builder.Configuration);

// App services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUploadService, LocalUploadService>();
builder.Services.AddScoped<SimpleRateLimitMiddleware>();
// Storage service provider selection
var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "Local";
if (storageProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<PublishRealLiteApi.Services.Interfaces.IStorageService, PublishRealLiteApi.Services.AzureBlobStorageService>();
}
else
{
    builder.Services.AddScoped<PublishRealLiteApi.Services.Interfaces.IStorageService, PublishRealLiteApi.Services.LocalStorageService>();
}

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSection.GetValue<string>("Secret") ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
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
    options.RequireHttpsMetadata = true;
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

// CORS
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

// Rate limiting and memory cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SimpleRateLimitMiddleware>();
// HttpContext accessor and current user service for auditing
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PublishRealLiteApi.Services.ICurrentUserService, PublishRealLiteApi.Services.CurrentUserService>();
// Background worker for stats aggregation
builder.Services.AddHostedService<PublishRealLiteApi.Workers.StreamStatAggregatorWorker>();
// Aggregator service for manual invocation
builder.Services.AddScoped<PublishRealLiteApi.Services.Interfaces.IStreamStatAggregatorService, PublishRealLiteApi.Services.StreamStatAggregatorService>();

var app = builder.Build();

// Middleware pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SimpleRateLimitMiddleware>();

// OpenAPI + Scalar UI
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

// Health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.MapControllers();

// Diagnostic endpoint to trigger aggregation on demand (admin use)
app.MapPost("/admin/aggregate-stats", async (PublishRealLiteApi.Services.Interfaces.IStreamStatAggregatorService svc, CancellationToken ct) =>
{
    await svc.AggregateOnceAsync(ct);
    return Results.Ok(new { message = "Aggregation executed" });
}).RequireAuthorization();

// Database migration + seeding at startup with enhanced diagnostics
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
        // E: List IEntityTypeConfiguration<> implementations across PublishRealLiteApi assemblies for diagnostics
        try
        {
            var asmList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.FullName != null && a.FullName.StartsWith("PublishRealLiteApi", StringComparison.OrdinalIgnoreCase))
                .ToList();

            logger.LogInformation("Scanning {Count} assemblies for IEntityTypeConfiguration<> implementations...", asmList.Count);

            var found = new List<string>();
            foreach (var asm in asmList)
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    types = rtle.Types.Where(t => t != null).ToArray()!;
                }

                foreach (var t in types.Where(t => t != null && !t.IsAbstract && !t.IsInterface))
                {
                    var ifaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
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

            if (!found.Any()) logger.LogInformation("No IEntityTypeConfiguration<> implementations found in PublishRealLiteApi assemblies.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error while scanning assemblies for IEntityTypeConfiguration<> implementations");
        }
    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        // Apply migrations or create schema depending on whether migrations exist
        var allMigrations = db.Database.GetMigrations().ToList();
        if (!allMigrations.Any())
        {
            // No migrations in the project: use EnsureCreated for first-run schema creation
            if (!db.Database.CanConnect())
            {
                logger.LogInformation("No migrations found and database cannot be reached. Creating database schema with EnsureCreated()...");
                db.Database.EnsureCreated();
            }
            else
            {
                var applied = db.Database.GetAppliedMigrations().ToList();
                if (!applied.Any())
                {
                    logger.LogInformation("No migrations present and no applied migrations found. Creating schema with EnsureCreated()...");
                    db.Database.EnsureCreated();
                }
                else
                {
                    logger.LogInformation("No migrations in project, but database already has schema. Skipping creation.");
                }
            }
        }
        else
        {
            // There are migrations: apply them when needed
            if (!db.Database.CanConnect())
            {
                logger.LogInformation("Database does not exist or cannot be reached. Creating database and applying migrations...");
                db.Database.Migrate();
            }
            else
            {
                var applied = db.Database.GetAppliedMigrations().ToList();

                // If there are no applied migrations but the DB already contains tables (created outside migrations)
                // skip automatic Migrate to avoid "object already exists" errors and log actionable guidance.
                if (!applied.Any())
                {
                    bool dbHasTables = false;
                    try
                    {
                        var conn = db.Database.GetDbConnection();
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES";
                        var res = cmd.ExecuteScalar();
                        conn.Close();
                        dbHasTables = Convert.ToInt32(res) > 0;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not determine if database already has tables; skipping automatic baseline detection.");
                    }

                    if (dbHasTables)
                    {
                        logger.LogWarning("Database contains existing schema but no applied EF migrations were detected. Skipping automatic Migrate() to avoid creating objects that already exist.");
                        logger.LogWarning("Action recommended: create a baseline migration and mark it as applied, or generate migrations with 'ignore changes' so EF Core knows the DB is already in the expected state.");
                        logger.LogWarning("Examples:");
                        logger.LogWarning(" - In Package Manager Console (Visual Studio): Add-Migration InitialCreate -IgnoreChanges");
                        logger.LogWarning(" - Or using dotnet-ef: generate a migration then edit it to be empty, or use project-specific instructions to create a baseline.");
                        logger.LogWarning("Also for production: replace EnsureCreated() by using migrations exclusively and apply them in a controlled deployment (use 'dotnet ef database update' in CI/CD).\n");

                        // Additional guidance as requested by reviewer
                        logger.LogWarning("Notes:");
                        logger.LogWarning(" - For production: replace EnsureCreated() by using only migrations (create the initial migration and use Migrate in controlled deployments).");
                        logger.LogWarning(" - The worker is a prototype: for scale consider a message queue (Kafka, Event Hubs) or a TSDB like TimescaleDB for raw ingestion.");
                        logger.LogWarning(" - To force aggregation via HTTP you can convert /admin/aggregate-stats to call the worker's aggregation method directly (AggregateAsync).");
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
                            logger.LogInformation("No pending migrations. Database is up-to-date.");
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
                        logger.LogInformation("No pending migrations. Database is up-to-date.");
                    }
                }
            }
        }

        // Seed initial data
        await DbSeeder.SeedAsync(services);
    }
    catch (InvalidOperationException invEx) when (
    invEx.Message?.Contains("resolved to System.Object") == true ||
    invEx.Message?.Contains("entity type 'object'") == true ||
    invEx.Message?.Contains("requires a primary key") == true)
    {
        logger.LogError(invEx, "Startup migration failed due to EF model validation (possible 'object' entity mapping).");

        // Scan assemblies for IEntityTypeConfiguration<> implementations and print resolved generic arguments
        try
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            foreach (var asm in assemblies)
            {
                logger.LogInformation("Scanning assembly {Asm} for IEntityTypeConfiguration<> implementations...", asm.FullName);

                var configs = asm.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Select(t => new
                    {
                        Type = t,
                        Interfaces = t.GetInterfaces()
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
                            .ToArray()
                    })
                    .Where(x => x.Interfaces.Length > 0)
                    .ToList();

                if (!configs.Any())
                {
                    logger.LogInformation("No IEntityTypeConfiguration<> implementations found in this assembly.");
                }
                else
                {
                    foreach (var cfg in configs)
                    {
                        foreach (var iface in cfg.Interfaces)
                        {
                            var entityType = iface.GetGenericArguments()[0];
                            logger.LogInformation("Config: {Cfg} -> IEntityTypeConfiguration<{Entity}>", cfg.Type.FullName, entityType.FullName);
                        }
                    }
                }
            }
        }
        catch (Exception scanEx)
        {
            logger.LogError(scanEx, "Error while scanning for configuration types");
        }

        // Re-throw to preserve original failure behavior
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup migration/seeding error");
        throw;
    }
}

app.Run();
