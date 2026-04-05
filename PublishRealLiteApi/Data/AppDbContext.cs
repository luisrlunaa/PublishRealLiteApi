using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Data
{
    // IdentityDbContext integrates Identity with EF Core 
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ArtistProfile> ArtistProfiles { get; set; } = null!;
        public DbSet<Release> Releases { get; set; } = null!;
        public DbSet<Track> Tracks { get; set; } = null!;
        public DbSet<StreamStat> StreamStats { get; set; }
        public DbSet<ArtistVideo> ArtistVideos { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamInvite> TeamInvites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity
            builder.Entity<IdentityUser>().HasIndex("NormalizedEmail");

            // ArtistProfile
            builder.Entity<ArtistProfile>(b =>
            {
                b.HasKey(p => p.Id);

                b.HasOne(p => p.User)
                 .WithOne()
                 .HasForeignKey<ArtistProfile>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Releases
                b.HasMany(p => p.Releases)
                 .WithOne(r => r.ArtistProfile)
                 .HasForeignKey(r => r.ArtistProfileId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Videos (asegúrate de que ArtistProfile tiene ICollection<ArtistVideo> Videos)
                b.HasMany(p => p.Videos)
                 .WithOne(v => v.ArtistProfile)
                 .HasForeignKey(v => v.ArtistProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Release
            builder.Entity<Release>(r =>
            {
                r.HasKey(x => x.Id);
                r.Property(x => x.Title).IsRequired();
            });

            // Track
            builder.Entity<Track>(t =>
            {
                t.HasKey(x => x.Id);
                t.HasIndex(x => new { x.ReleaseId, x.Position }).IsUnique();
            });

            // ArtistVideo
            builder.Entity<ArtistVideo>(v =>
            {
                v.HasKey(x => x.Id);
                v.Property(x => x.Title).HasMaxLength(300).IsRequired();
                v.Property(x => x.ThumbnailUrl).HasMaxLength(500);
                v.Property(x => x.VideoUrl).HasMaxLength(1000);
            });

            // StreamStat
            builder.Entity<StreamStat>(s =>
            {
                s.HasKey(x => x.Id);
                s.Property(x => x.Platform).HasMaxLength(50).IsRequired();
                s.Property(x => x.Country).HasMaxLength(100).IsRequired();
                s.Property(x => x.MetricType).HasMaxLength(50).IsRequired();
                s.Property(x => x.Source).HasMaxLength(200);
            });

            // Team
            builder.Entity<Team>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).HasMaxLength(200);
            });

            builder.Entity<TeamMember>(tm =>
            {
                tm.HasKey(x => x.Id);
                tm.Property(x => x.Email).HasMaxLength(256).IsRequired();
            });

            builder.Entity<TeamInvite>(ti =>
            {
                ti.HasKey(x => x.Id);
                ti.Property(x => x.Email).HasMaxLength(256).IsRequired();
                ti.Property(x => x.Token).HasMaxLength(200).IsRequired();
                ti.HasIndex(x => x.Token).IsUnique();
            });
        }

    }
}