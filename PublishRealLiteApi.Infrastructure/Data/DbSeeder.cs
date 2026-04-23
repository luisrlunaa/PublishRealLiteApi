using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Data
{
    public static class DbSeeder
    {
        // Initial seed: roles, admin and demo artist 
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var db = services.GetRequiredService<AppDbContext>();

            // Ensure the database schema exists before attempting Identity queries.
            // Defensive: if migrations exist apply them; otherwise create schema for first-run.
            try
            {
                await EnsureDatabaseSchemaAsync(db);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring database schema before seeding: {ex}");
                throw;
            }

            // Create roles if they don't exist 
            var roles = new[] { "Admin", "Artist" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                {
                    await roleManager.CreateAsync(new IdentityRole(r));
                }
            }

            // Admin 
            var adminEmail = "admin@publishreal.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true};
                var res = await userManager.CreateAsync(admin, "AdminPass!23");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Demo artist 
            var artistEmail = "artist1@publishreal.local";
            if (await userManager.FindByEmailAsync(artistEmail) == null)
            {
                var artistUser = new IdentityUser { UserName = artistEmail, Email = artistEmail, EmailConfirmed = true};
                var res = await userManager.CreateAsync(artistUser, "Password123!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(artistUser, "Artist");

                    // Create profile and release demo 
                    var profile = new ArtistProfile
                    {
                        UserId = artistUser.Id,
                        ArtistName = "LunasSystems",
                        Bio = "Software & Sound Engineer",
                        ProfileImageUrl = null,
                        SocialLinksJson = "{\"instagram\":\"https://instagram.com/lunas\"}"
                    };
                    db.ArtistProfiles.Add(profile);

                    var release = new Release
                    {
                        ArtistProfile = profile,
                        Title = "Alpha Release",
                        ReleaseDate = DateTime.UtcNow.Date,
                        Genre = "Synthwave",
                        Label = "Indie",
                        UPC = "123456789012",
                        ISRC = null,
                        LinksJson = "{\"spotify\":\"https://open.spotify.com/album/placeholder\"}"
                    };
                    db.Releases.Add(release);

                    db.Tracks.Add(new Track { Release = release, Position = 1, Title = "Track 1" });
                    db.Tracks.Add(new Track { Release = release, Position = 2, Title = "Track 2" });

                    db.StreamStats.AddRange(new[]
                    {
                        new StreamStat
                        {
                            Date = DateTime.UtcNow.Date.AddDays(-1),
                            Platform = "Spotify",
                            Country = "US",
                            Streams = 120,
                            MetricType = "streams",
                            Source = "playlist"
                        },
                        new StreamStat
                        {
                            Date = DateTime.UtcNow.Date.AddDays(-1),
                            Platform = "Spotify",
                            Country = "MX",
                            Streams = 45,
                            MetricType = "streams",
                            Source = "radio"
                        }
                    });

                    await db.SaveChangesAsync();
                }
            }
        }

        private static Task EnsureDatabaseSchemaAsync(AppDbContext db)
        {
            // If the project contains migrations, apply them. Otherwise use EnsureCreated for initial schema.
            var allMigrations = db.Database.GetMigrations().ToList();
            if (!allMigrations.Any())
            {
                // No migrations defined in the project
                var applied = db.Database.GetAppliedMigrations().ToList();
                if (!db.Database.CanConnect() || !applied.Any())
                {
                    Console.WriteLine("No migrations found. Creating database schema with EnsureCreated()...");
                    db.Database.EnsureCreated();
                }
                else
                {
                    Console.WriteLine("No migrations found but database already has applied migrations/schema. Skipping creation.");
                }
            }
            else
            {
                // There are migrations in the project. Be defensive: if none are applied but the DB already
                // contains tables, skip automatic Migrate() to avoid "object already exists" errors.
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
                        var res = cmd.ExecuteScalar();
                        conn.Close();
                        dbHasTables = Convert.ToInt32(res) > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not determine if database already has tables: " + ex.Message);
                    }

                    if (dbHasTables)
                    {
                        Console.WriteLine("Database contains existing schema but no applied EF migrations were detected. Skipping automatic Migrate() to avoid creating objects that already exist.");
                        Console.WriteLine("Action recommended: create a baseline migration (e.g. using -IgnoreChanges) or mark the current schema as the baseline before applying migrations.");
                    }
                    else
                    {
                        Console.WriteLine("Migrations detected. Applying migrations...");
                        try
                        {
                            db.Database.Migrate();
                        }
                        catch (SqlException sqlex)
                        {
                            var msg = sqlex.Message ?? string.Empty;
                            if (msg.Contains("There is already an object named", StringComparison.OrdinalIgnoreCase) || msg.Contains("already an object named", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Migration failed because some database objects already exist. This can happen if the database was previously created with EnsureCreated() or manually.");
                                Console.WriteLine("Recommended actions:");
                                Console.WriteLine(" - Create an initial baseline migration and mark it as applied (use -IgnoreChanges or generate an empty migration and commit it).");
                                Console.WriteLine(" - Or drop the database and let migrations create it if you are in development and data loss is acceptable.");
                                Console.WriteLine(" - Check the __EFMigrationsHistory table to see applied migrations.");
                                Console.WriteLine("SqlException: " + sqlex.Message);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Migrations detected. Applying migrations...");
                    db.Database.Migrate();
                }
            }

            return Task.CompletedTask;
        }
    }
}
