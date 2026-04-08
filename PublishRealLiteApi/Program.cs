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

// Database migration + seeding at startup with enhanced diagnostics
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
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
                Console.WriteLine("No migrations found and database cannot be reached. Creating database schema with EnsureCreated()...");
                db.Database.EnsureCreated();
            }
            else
            {
                var applied = db.Database.GetAppliedMigrations().ToList();
                if (!applied.Any())
                {
                    Console.WriteLine("No migrations present and no applied migrations found. Creating schema with EnsureCreated()...");
                    db.Database.EnsureCreated();
                }
                else
                {
                    Console.WriteLine("No migrations in project, but database already has schema. Skipping creation.");
                }
            }
        }
        else
        {
            // There are migrations: apply them when needed
            if (!db.Database.CanConnect())
            {
                Console.WriteLine("Database does not exist or cannot be reached. Creating database and applying migrations...");
                db.Database.Migrate();
            }
            else
            {
                var pending = db.Database.GetPendingMigrations().ToList();
                if (pending.Any())
                {
                    Console.WriteLine($"Applying {pending.Count} pending migration(s)...");
                    db.Database.Migrate();
                }
                else
                {
                    Console.WriteLine("No pending migrations. Database is up-to-date.");
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
        Console.WriteLine("Startup migration failed due to EF model validation (possible 'object' entity mapping).");
        Console.WriteLine("Exception: " + invEx);

        // Scan assemblies for IEntityTypeConfiguration<> implementations and print resolved generic arguments
        try
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            foreach (var asm in assemblies)
            {
                Console.WriteLine($"Scanning assembly {asm.FullName} for IEntityTypeConfiguration<> implementations...");

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
                    Console.WriteLine("No IEntityTypeConfiguration<> implementations found in this assembly.");
                }
                else
                {
                    foreach (var cfg in configs)
                    {
                        foreach (var iface in cfg.Interfaces)
                        {
                            var entityType = iface.GetGenericArguments()[0];
                            Console.WriteLine($"Config: {cfg.Type.FullName} -> IEntityTypeConfiguration<{entityType.FullName}>");
                        }
                    }
                }
            }
        }
        catch (Exception scanEx)
        {
            Console.WriteLine("Error while scanning for configuration types: " + scanEx);
        }

        // Re-throw to preserve original failure behavior
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Startup migration/seeding error: " + ex);
        throw;
    }
}

app.Run();
