using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PublishRealLiteApi.Data;
using PublishRealLiteApi.Models;
using PublishRealLiteApi.Services;
using Microsoft.Win32;
using System.Text;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ---------------------------------------------------------
// ADVANCED SQL SERVER INSTANCE DETECTOR (NOT WMI)
// ---------------------------------------------------------
string DetectSqlServerInstance()
{
    var instances = new List<string>();

    // 1. Read instances from the registry 
    try
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");
        if (key != null)
        {
            foreach (var name in key.GetValueNames())
            {
                if (name == "MSSQLSERVER")
                    instances.Add(".");
                else
                    instances.Add($".\\{name}");
            }
        }
    }
    catch { }

    // 2. Common Instances 
    var fallback = new[]
    {
".", "localhost", "(local)",
".\\SQLEXPRESS",
$"{Environment.MachineName}",
$"{Environment.MachineName}\\SQLEXPRESS"
};

    instances.AddRange(fallback);

    // 3. Test each instance 
    foreach (var server in instances.Distinct())
    {
        try
        {
            var testConn = $"Server={server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(testConn);
            conn.Open();
            return server;
        }
        catch { }
    }

    throw new Exception("No valid SQL Server instance found.");
}

string BuildDynamicConnectionString()
{
    var baseConn = configuration.GetConnectionString("DefaultConnection");

    if (!baseConn.Contains("Server=DYNAMIC"))
        throw new Exception("The connection string must contain 'Server=DYNAMIC'.");

    var server = DetectSqlServerInstance();
    return baseConn.Replace("Server=DYNAMIC", $"Server={server}");
}

var finalConnectionString = BuildDynamicConnectionString();

// ---------------------------------------------------------
// DATABASE + IDENTITY + JWT CONFIG
// ---------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(finalConnectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtSection = configuration.GetSection("JwtSettings");
var secret = jwtSection.GetValue<string>("Secret")!;
var issuer = jwtSection.GetValue<string>("Issuer")!;
var audience = jwtSection.GetValue<string>("Audience")!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateLifetime = true
    };
});

builder.Services.AddOpenApi(); // ✔️ Native OpenAPI
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod()
        .WithOrigins("http://localhost:5173", "http://localhost:3000")
        .AllowCredentials();
    });
});

var app = builder.Build();

// ---------------------------------------------------------
// AUTO DB CREATION + MIGRATIONS + SEED
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        if (!db.Database.CanConnect())
            db.Database.EnsureCreated();

        db.Database.Migrate();

        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error initializing database");
        throw;
    }
}

// OpenAPI JSON
app.MapOpenApi();

// Scalar UI
app.MapScalarApiReference(options =>
{
    options.Title = "PublishRealLite API";
    options.Theme = ScalarTheme.DeepSpace;
});

app.UseHttpsRedirection();
app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();