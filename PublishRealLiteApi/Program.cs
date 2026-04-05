using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.Services;
using PublishRealLiteApi.Services.Interfaces;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Configuration helpers
// -----------------------------
var configuration = builder.Configuration;

// -----------------------------
// Database
// -----------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(conn, sql =>
    {
        sql.EnableRetryOnFailure();
    });
});

// -----------------------------
// Identity
// -----------------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -----------------------------
// JWT Authentication
// -----------------------------
var jwtSection = configuration.GetSection("JwtSettings");
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

// -----------------------------
// CORS
// -----------------------------
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

// -----------------------------
// Controllers, OpenAPI, Scalar UI, HealthChecks
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // native OpenAPI registration (Microsoft.AspNetCore.OpenApi) 
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("Database");

// -----------------------------
// Application services (DI)
// -----------------------------
builder.Services.AddScoped<IUploadService, LocalUploadService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, PublishRealLiteApi.Services.NullEmailSender>();
builder.Services.AddScoped<DatabaseHealthCheck>();


// Register other app services you implemented
// builder.Services.AddScoped<IReleaseService, ReleaseService>();
// builder.Services.AddScoped<IVideoService, VideoService>();

// -----------------------------
// Simple in-memory rate limiting middleware registration
// -----------------------------
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SimpleRateLimitMiddleware>();

// -----------------------------
// Build app
// -----------------------------
var app = builder.Build();

// -----------------------------
// Middleware pipeline
// -----------------------------
app.UseStaticFiles(); // serve wwwroot/uploads for images

app.UseRouting();

app.UseCors("FrontendDev");

app.UseAuthentication();
app.UseAuthorization();

// Simple rate limiting middleware (global)
app.UseMiddleware<SimpleRateLimitMiddleware>();

// OpenAPI + Scalar UI
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "PublishRealLite API";
    options.Theme = ScalarTheme.DeepSpace;
});

// Optional: redirect root to Scalar UI
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1");
    return Task.CompletedTask;
});

// Controllers and health
app.MapControllers();
app.MapHealthChecks("/health");

// -----------------------------
// Database migration + seeding at startup
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        // Apply pending migrations
        db.Database.Migrate();

        // Seed initial data (roles, admin, demo artist, optional demo stats)
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
    }
}

// -----------------------------
// Run
// -----------------------------
app.Run();