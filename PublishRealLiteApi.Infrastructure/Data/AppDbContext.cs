using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PublishRealLiteApi.Infrastructure.Identity;
using PublishRealLiteApi.Models;
using System.Reflection;

namespace PublishRealLiteApi.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<ArtistProfile> ArtistProfiles => Set<ArtistProfile>();
        public DbSet<Release> Releases => Set<Release>();
        public DbSet<Track> Tracks => Set<Track>();
        public DbSet<ArtistVideo> ArtistVideos => Set<ArtistVideo>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<TeamInvite> TeamInvites => Set<TeamInvite>();
        public DbSet<StreamStat> StreamStats => Set<StreamStat>();
        public DbSet<PublishRealLiteApi.Models.StreamStatDailyAggregate> StreamStatDailyAggregates => Set<PublishRealLiteApi.Models.StreamStatDailyAggregate>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Diagnostic: list mapped entity CLR types
            var mapped = builder.Model.GetEntityTypes()
                .Select(e => new { Name = e.Name, Clr = e.ClrType?.FullName ?? "<null>" })
                .OrderBy(x => x.Name)
                .ToList();

            _logger.LogInformation("Mapped entity types (diagnostic):");
            foreach (var m in mapped)
            {
                _logger.LogInformation(" - {Name} => {Clr}", m.Name, m.Clr);
            }

            // Apply filtered configurations from all relevant loaded assemblies (strict, safe)
            ApplyFilteredConfigurations(builder);

            // Explicit mappings (your existing rules)
            builder.Entity<ArtistProfile>(b =>
            {
                b.HasKey(p => p.Id);

                // Map to Identity user (AppUser) without requiring a CLR navigation property
                b.HasOne<AppUser>()
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
                // Composite index for efficient queries
                s.HasIndex(x => new { x.ArtistProfileId, x.Date, x.Platform });
                s.HasIndex(x => new { x.Date, x.Platform });
            });

            builder.Entity<PublishRealLiteApi.Models.StreamStatDailyAggregate>(agg =>
            {
                agg.HasKey(a => a.Id);
                agg.HasIndex(a => new { a.ArtistProfileId, a.Date, a.Platform }).IsUnique(false);
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

            // Strict validation: detect entities without key that belong to your domain and fail with clear message
            var typesWithoutKey = builder.Model.GetEntityTypes()
                .Where(e => e.FindPrimaryKey() == null)
                .Select(e => new { e.Name, Clr = e.ClrType })
                .ToList();

            var problematic = typesWithoutKey
                .Where(x =>
                    x.Clr != null &&
                    x.Clr != typeof(object) &&
                    (x.Clr.Namespace != null && x.Clr.Namespace.StartsWith("PublishRealLiteApi", StringComparison.OrdinalIgnoreCase)))
                .Select(x => new { x.Name, Namespace = x.Clr.Namespace })
                .ToList();

            if (typesWithoutKey.Any(t => t.Clr == typeof(object)))
            {
                _logger.LogWarning("EF Core detected an entity mapped as 'object'. This usually means a configuration class targets System.Object or a configuration failed to resolve the entity type.");
                _logger.LogWarning("Run the diagnostic helper to find types implementing IEntityTypeConfiguration<object> or invalid configurations.");
            }

            if (problematic.Any())
            {
                var list = string.Join(", ", problematic.Select(p => $"{p.Name} ({p.Namespace})"));
                throw new InvalidOperationException($"EF model contains entity types without primary key (fix these models): {list}");
            }
        }

        public override int SaveChanges()
        {
            ApplyAuditProperties();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditProperties();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditProperties()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity != null && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            var now = DateTime.UtcNow;
            foreach (var e in entries)
            {
                var propUpdatedAt = e.Metadata.FindProperty("UpdatedAt");
                if (propUpdatedAt != null)
                {
                    e.CurrentValues["UpdatedAt"] = now;
                }

                if (e.State == EntityState.Added)
                {
                    var createdAt = e.Metadata.FindProperty("CreatedAt");
                    if (createdAt != null)
                    {
                        e.CurrentValues["CreatedAt"] = now;
                    }

                    var createdBy = e.Metadata.FindProperty("CreatedBy");
                    if (createdBy != null)
                    {
                        // CreatedBy will be set by the application when available. Leave null in automated/CLI contexts.
                    }
                }

                if (e.State == EntityState.Deleted)
                {
                    // Soft delete: set IsDeleted and DeletedAt, change state to Modified
                    var isDeleted = e.Metadata.FindProperty("IsDeleted");
                    if (isDeleted != null)
                    {
                        e.CurrentValues["IsDeleted"] = true;
                        var deletedAt = e.Metadata.FindProperty("DeletedAt");
                        if (deletedAt != null) e.CurrentValues["DeletedAt"] = now;
                        e.State = EntityState.Modified;
                    }
                }
            }
        }

        // Hardened ApplyFilteredConfigurations: handles multiple IEntityTypeConfiguration<> interfaces,
        // logs every mapping decision, and collects any configurations that resolve to System.Object.
        private void ApplyFilteredConfigurations(ModelBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.FullName != null && a.FullName.StartsWith("PublishRealLiteApi", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Scanning {assemblies.Count} PublishRealLiteApi assembly(ies) for IEntityTypeConfiguration<> implementations...");

            var offendingConfigs = new System.Collections.Generic.List<string>();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    types = rtle.Types.Where(t => t != null).ToArray()!; // proceed with loadable types
                }

                var configTypes = types
                    .Where(t => t != null && !t.IsAbstract && !t.IsInterface)
                    .Select(t => new
                    {
                        Type = t!,
                        Interfaces = t!.GetInterfaces()
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                            .ToArray()
                    })
                    .Where(x => x.Interfaces.Length > 0)
                    .ToList();

                Console.WriteLine($"Found {configTypes.Count} configuration type(s) in assembly {assembly.FullName}.");

                foreach (var cfg in configTypes)
                {
                    foreach (var iface in cfg.Interfaces)
                    {
                        Type? entityType = null;
                        try
                        {
                            entityType = iface.GetGenericArguments()[0];

                            Console.WriteLine($"Configuration type: {cfg.Type.FullName} -> resolved entity: {entityType?.FullName ?? "<null>"}");

                            if (entityType == null || entityType == typeof(object))
                            {
                                _logger.LogWarning("Skipping configuration {Cfg} because it targets {Entity}", cfg.Type.FullName, entityType?.FullName ?? "null");
                                offendingConfigs.Add($"{cfg.Type.FullName} -> {entityType?.FullName ?? "<null>"}");
                                continue;
                            }

                            if (entityType.Namespace == null || !entityType.Namespace.StartsWith("PublishRealLiteApi.Models", StringComparison.Ordinal))
                            {
                                _logger.LogInformation("Skipping configuration {Cfg} because entity {Entity} is outside domain namespace.", cfg.Type.FullName, entityType.FullName);
                                continue;
                            }

                            var applyConfigMethod = typeof(ModelBuilder).GetMethods()
                                .First(m => m.Name == "ApplyConfiguration" && m.GetParameters().Length == 1)
                                .MakeGenericMethod(entityType);

                            var instance = Activator.CreateInstance(cfg.Type);
                            if (instance == null)
                            {
                                Console.WriteLine($"Could not create instance of configuration type {cfg.Type.FullName}; skipping.");
                                continue;
                            }

                            applyConfigMethod.Invoke(builder, new[] { instance });
                            _logger.LogInformation("Applied configuration {Cfg} for entity {Entity}", cfg.Type.FullName, entityType.FullName);
                        }
                        catch (TargetInvocationException tie)
                        {
                            Console.WriteLine($"Error applying configuration {cfg.Type.FullName} for entity '{entityType?.FullName ?? "unknown"}': {tie.InnerException?.Message ?? tie.Message}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error while applying configuration {cfg.Type.FullName}: {ex.Message}");
                            continue;
                        }
                    }
                }
            }

            if (offendingConfigs.Any())
            {
                var details = string.Join("; ", offendingConfigs);
                // Fail early with a clear message listing offending configuration types
                throw new InvalidOperationException($"One or more IEntityTypeConfiguration<> implementations resolved to System.Object or null. Offenders: {details}");
            }
        }
    }
}
