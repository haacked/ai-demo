using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Entities;

public class AIDemoContext(DbContextOptions<AIDemoContext> options) : DbContext(options)
{
    public const string ConnectionStringName = "AIDemoContext";

    public DbSet<User> Users { get; init; } = null!;

    public DbSet<Contact> Contacts { get; init; } = null!;

    public DbSet<AssistantThread> Threads { get; init; } = null!;

    public DbSet<UserFact> UserFacts { get; init; } = null!;

    public DbSet<ContactFact> ContactFacts { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //
        // IMPORTANT! Custom crap goes AFTER the call to base.OnModelCreating
        //

        modelBuilder.HasPostgresExtension("citext"); // case insensitive text
        modelBuilder.HasPostgresExtension("vector"); // pgvector for vector similarity support.
        modelBuilder.HasPostgresExtension("postgis");

        // User names must be unique.
        modelBuilder.Entity<User>()
            .HasIndex(i => new { Name = i.NameIdentifier })
            .IsUnique();

        modelBuilder.Entity<UserFact>()
            .HasIndex(i => new { i.UserId, i.Content })
            .IsUnique();

        modelBuilder.Entity<ContactFact>()
            .HasIndex(i => new { i.ContactId, i.Content })
            .IsUnique();

        modelBuilder.Entity<Contact>()
            .OwnsMany(
                c => c.EmailAddresses,
                c =>
                {
                    c.WithOwner().HasForeignKey("ContactId");
                    c.Property<int>("Id");
                    c.HasKey("Id");
                });

        modelBuilder.Entity<Contact>()
            .OwnsMany(
                c => c.Addresses,
                c =>
                {
                    c.WithOwner().HasForeignKey("ContactId");
                    c.Property<int>("Id");
                    c.HasKey("Id");
                });

        modelBuilder.Entity<Contact>()
            .OwnsMany(
                c => c.Names,
                c =>
                {
                    c.WithOwner().HasForeignKey("ContactId");
                    c.Property<int>("Id");
                    c.HasKey("Id");
                });
    }
}