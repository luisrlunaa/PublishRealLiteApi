using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Models;
using System.Reflection.Emit;

namespace PublishRealLiteApi.Data
{
    // IdentityDbContext integra Identity con EF Core
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ArtistProfile> ArtistProfiles { get; set; } = null!;
        public DbSet<Release> Releases { get; set; } = null!;
        public DbSet<Track> Tracks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique email enforced by Identity, pero añadimos índice por seguridad
            builder.Entity<IdentityUser>().HasIndex("NormalizedEmail");

            // ArtistProfile: one-to-one with AppUser (UserId is string from Identity)
            builder.Entity<ArtistProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<ArtistProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Release -> ArtistProfile (many)
            builder.Entity<Release>()
                .HasOne(r => r.ArtistProfile)
                .WithMany(p => p.Releases)
                .HasForeignKey(r => r.ArtistProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Track -> Release (many)
            builder.Entity<Track>()
                .HasOne(t => t.Release)
                .WithMany(r => r.Tracks)
                .HasForeignKey(t => t.ReleaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: position per release
            builder.Entity<Track>()
                .HasIndex(t => new { t.ReleaseId, t.Position })
                .IsUnique();
        }
    }
}
