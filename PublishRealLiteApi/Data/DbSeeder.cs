using Microsoft.AspNetCore.Identity;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Data
{
    public static class DbSeeder
    {
        // Initial seed: roles, admin and demo artist 
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var db = services.GetRequiredService<AppDbContext>();

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
                var admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
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
                var artistUser = new AppUser { UserName = artistEmail, Email = artistEmail, EmailConfirmed = true };
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

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}