using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A collection of EF Core entities with an <see cref="IsLoaded"/> property that lets us know if the collection
/// was loaded.
/// </summary>
/// <remarks>
/// Credit to Arthur Vickers from the EF Core team for sketching out the implementation in
/// https://github.com/dotnet/efcore/issues/29256
/// </remarks>
/// <typeparam name="T"></typeparam>
public class EntityList<T> : List<T>
{
    readonly DbContext? _context;
    readonly object? _parent;
    readonly string? _navigation;

    public EntityList()
    {
    }

    /// <summary>
    /// Constructs an <see cref="EntityList{T}"/> with a <see cref="DbContext"/> injected by EF.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="parent">The parent object this collection is a property of.</param>
    /// <param name="navigation">The name of the navigation.</param>
    // ReSharper disable once UnusedMember.Global
    public EntityList(DbContext context, object parent, string navigation)
    {
        _context = context;
        _parent = parent;
        _navigation = navigation;
    }

    /// <summary>
    /// If <c>true</c>, this collection is loaded.
    /// </summary>
    public bool IsLoaded => _context?.Entry(_parent!).Collection(_navigation!).IsLoaded ?? false;
}
