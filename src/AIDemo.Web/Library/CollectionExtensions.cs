using System.Collections.ObjectModel;
using MassTransit.Internals;
namespace Haack.AIDemoWeb.Library;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static async Task<IReadOnlyList<TElement>> ToReadOnlyListAsync<TElement>(
        this IAsyncEnumerable<TElement> elements, CancellationToken cancellationToken = default)
        where TElement : class
    {
        return new ReadOnlyCollection<TElement>(await elements.ToListAsync(cancellationToken));
    }
}