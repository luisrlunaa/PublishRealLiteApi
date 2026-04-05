using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Infrastructure.Identity;
using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ArtistProfile> ArtistProfiles => Set<ArtistProfile>();
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<ArtistVideo> ArtistVideos => Set<ArtistVideo>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamInvite> TeamInvites => Set<TeamInvite>();
    public DbSet<StreamStat> StreamStats => Set<StreamStat>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1) Aplicar solo configuraciones válidas del ensamblado actual (filtrado por namespace de modelos)
        ApplyFilteredConfigurations(builder, Assembly.GetExecutingAssembly());

        // 2) Mapeos explícitos (mantener tus reglas)
        builder.Entity<ArtistProfile>(b =>
        {
            b.HasKey(p => p.Id);

            b.HasOne(p => p.User)
             .WithOne()
             .HasForeignKey<ArtistProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Releases)
             .WithOne(r => r.ArtistProfile)
             .HasForeignKey(r => r.ArtistProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Videos)
             .WithOne(v => v.ArtistProfile)
             .HasForeignKey(v => v.ArtistProfileId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Release>(r =>
        {
            r.HasKey(x => x.Id);
            r.Property(x => x.Title).IsRequired();
        });

        builder.Entity<Track>(t =>
        {
            t.HasKey(x => x.Id);
            t.HasIndex(x => new { x.ReleaseId, x.Position }).IsUnique();
        });

        builder.Entity<ArtistVideo>(v =>
        {
            v.HasKey(x => x.Id);
            v.Property(x => x.Title).HasMaxLength(300).IsRequired();
            v.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            v.Property(x => x.VideoUrl).HasMaxLength(1000);
        });

        builder.Entity<StreamStat>(s =>
        {
            s.HasKey(x => x.Id);
            s.Property(x => x.Platform).HasMaxLength(50).IsRequired();
            s.Property(x => x.Country).HasMaxLength(100).IsRequired();
            s.Property(x => x.MetricType).HasMaxLength(50).IsRequired();
            s.Property(x => x.Source).HasMaxLength(200);
        });

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

        // 3) Validación estricta: detectar entidades sin clave que pertenezcan a tu dominio y fallar con mensaje claro
        var typesWithoutKey = builder.Model.GetEntityTypes()
            .Where(e => e.FindPrimaryKey() == null)
            .Select(e => new { e.Name, Clr = e.ClrType })
            .ToList();

        // Filtrar tipos del framework y System.Object; conservar solo los que pertenecen a tu código
        var problematic = typesWithoutKey
            .Where(x =>
                x.Clr != null &&
                x.Clr != typeof(object) &&
                (x.Clr.Namespace != null && x.Clr.Namespace.StartsWith("PublishRealLiteApi", StringComparison.OrdinalIgnoreCase)))
            .Select(x => x.Name)
            .ToList();

        if (problematic.Any())
        {
            var list = string.Join(", ", problematic);
            throw new InvalidOperationException($"EF model contains entity types without primary key (fix these configurations/models): {list}");
        }
    }

    private void ApplyFilteredConfigurations(ModelBuilder builder, Assembly assembly)
    {
        var configTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Type = t,
                Interface = t.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            })
            .Where(x => x.Interface != null)
            .ToList();

        foreach (var cfg in configTypes)
        {
            var entityType = cfg.Interface!.GetGenericArguments()[0];

            // Aplicar solo si la entidad pertenece al namespace de modelos de la aplicación
            if (entityType.Namespace != null && entityType.Namespace.StartsWith("PublishRealLiteApi.Models"))
            {
                var applyConfigMethod = typeof(ModelBuilder).GetMethods()
                    .First(m => m.Name == "ApplyConfiguration" && m.GetParameters().Length == 1)
                    .MakeGenericMethod(entityType);

                var instance = Activator.CreateInstance(cfg.Type);
                applyConfigMethod.Invoke(builder, new[] { instance! });
            }
        }
    }
}
