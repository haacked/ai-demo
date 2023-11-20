using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A user in the system.
/// </summary>
public class User
{
    public User()
    {
        Facts = new EntityList<UserFact>();
        Threads = new EntityList<AssistantThread>();
    }

    // Special constructor called by EF Core.
    // ReSharper disable once UnusedMember.Local
    public User(DbContext db)
    {
        Facts = new EntityList<UserFact>(db, this, nameof(Facts));
        Threads = new EntityList<AssistantThread>(db, this, nameof(Threads));
    }

    /// <summary>
    /// The primary key for the user.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The user name of the user.
    /// </summary>
    /// <remarks>
    /// User names must be unique in a case insensitive manner. That's what
    /// the `citext` ensures, that case doesn't matter.
    /// </remarks>
    [Column(TypeName = "citext")]
    public required string Name { get; set; }

    /// <summary>
    /// Facts about the user.
    /// </summary>
    public EntityList<UserFact> Facts { get; }

    /// <summary>
    /// The threads started by this user.
    /// </summary>
    public EntityList<AssistantThread> Threads { get; }
}