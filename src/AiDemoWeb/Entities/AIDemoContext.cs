using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Entities;

public class AIDemoContext : DbContext
{
    public const string ConnectionStringName = "AIDemoContext";

    public AIDemoContext(DbContextOptions<AIDemoContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //
        // IMPORTANT! Custom crap goes AFTER the call to base.OnModelCreating
        //

        modelBuilder.HasPostgresExtension("citext");

        // User names must be unique.
        modelBuilder.Entity<User>()
            .HasIndex(i => new { i.Name })
            .IsUnique();
    }
}