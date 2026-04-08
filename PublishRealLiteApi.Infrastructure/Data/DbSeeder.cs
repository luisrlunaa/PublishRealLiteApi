using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PublishRealLiteApi.Infrastructure.Identity;
using PublishRealLiteApi.Models;
using System;

namespace PublishRealLiteApi.Infrastructure.Data
{
    public static class DbSeeder
    {
        // Initial seed: roles, admin and demo artist 
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
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
                var admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, DisplayName = "Admin" };
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
                var artistUser = new AppUser { UserName = artistEmail, Email = artistEmail, EmailConfirmed = true, DisplayName = "Demo Artist" };
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
                Console.WriteLine("Migrations detected. Applying migrations...");
                db.Database.Migrate();
            }

            return Task.CompletedTask;
        }
    }
}
