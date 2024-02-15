using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Entities;

public class AIDemoContext(DbContextOptions<AIDemoContext> options) : DbContext(options)
{
    public const string ConnectionStringName = "AIDemoContext";

    public DbSet<User> Users { get; init; } = null!;

    public DbSet<AssistantThread> Threads { get; init; } = null!;

    public DbSet<UserFact> UserFacts { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //
        // IMPORTANT! Custom crap goes AFTER the call to base.OnModelCreating
        //

        modelBuilder.HasPostgresExtension("citext"); // case insensitive text
        modelBuilder.HasPostgresExtension("vector"); // pgvector for vector similarity support.

        // User names must be unique.
        modelBuilder.Entity<User>()
            .HasIndex(i => new { i.Name })
            .IsUnique();

        modelBuilder.Entity<UserFact>()
            .HasIndex(i => new { i.UserId, i.Content })
            .IsUnique();
    }
}