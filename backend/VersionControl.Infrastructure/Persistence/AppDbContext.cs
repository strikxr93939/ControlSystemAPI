using Microsoft.EntityFrameworkCore;
using VersionControl.Domain.Entities;

namespace VersionControl.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Computer> Computers => Set<Computer>();

    public DbSet<ProgramEntity> Programs => Set<ProgramEntity>();

    public DbSet<InstalledVersion> Versions => Set<InstalledVersion>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<Violation> Violations => Set<Violation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Computer>()
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.Entity<ProgramEntity>()
            .HasIndex(x => x.Name);

        builder.Entity<Violation>()
            .HasIndex(x => x.Timestamp);
    }
}
