using System.ComponentModel.DataAnnotations.Schema;

namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A user in the system.
/// </summary>
public class User
{
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
}