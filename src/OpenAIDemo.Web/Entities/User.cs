using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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
    /// The external identifier for this user. This is usually provided in the NameIdentifier claim.
    /// </summary>
    [Column(TypeName = "citext")]
    public required string NameIdentifier { get; set; }

    /// <summary>
    /// Facts about the user.
    /// </summary>
    public EntityList<UserFact> Facts { get; }

    /// <summary>
    /// The threads started by this user.
    /// </summary>
    public EntityList<AssistantThread> Threads { get; }

    /// <summary>
    /// The NetTopologySuite location of the user.
    /// </summary>
    [Column(TypeName = "geometry (point)")]
    public Point? Location { get; set; }

    /// <summary>
    /// The formatted address from the geo code service for the user.
    /// </summary>
    public string? FormattedAddress { get; set; }

    /// <summary>
    /// The IANA Time Zone identifier for the member.
    /// </summary>
    public string? TimeZoneId { get; set; }
}