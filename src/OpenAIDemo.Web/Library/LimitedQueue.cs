using System.Collections;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CA1711

namespace Serious;

/// <summary>
/// A queue with a limit
/// </summary>
public class LimitedQueue<TItem>(int limit) : IReadOnlyCollection<TItem>
{
    readonly Queue<TItem> _innerQueue = new();

    /// <summary>
    /// The maximum number of items that can be in the queue at any given time.
    /// </summary>
    public int Limit { get; } = limit;

    /// <summary>
    /// Adds an item to the tail of the queue. If this item would push the queue past the limit,
    /// the head of the queue is removed.
    /// </summary>
    /// <param name="item">The dequeued item.</param>
    public void Enqueue(TItem item)
    {
        _innerQueue.Enqueue(item);
        if (_innerQueue.Count > Limit)
        {
            _innerQueue.Dequeue();
        }
    }

    /// <summary>
    /// Removes and returns the object at the head of the queue (aka the oldest item added to the queue).
    /// </summary>
    /// <returns></returns>
    public TItem Dequeue() => _innerQueue.Dequeue();

    /// <summary>
    /// Tries to remove and return the object at the head of the queue (aka the oldest item added to the queue).
    /// </summary>
    /// <param name="result">The dequeued item.</param>
    /// <returns><c>true</c> if there was an item to dequeue.</returns>
    public bool TryDequeue([MaybeNullWhen(false)]out TItem result) => _innerQueue.TryDequeue(out result);

    /// <summary>
    /// Returns the object at the head of the queue. The object remains in the
    /// queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
    /// <returns></returns>
    public TItem Peek() => _innerQueue.Peek();

    public bool TryPeek([MaybeNullWhen(false)] out TItem result) => _innerQueue.TryPeek(out result);


    /// <summary>
    /// Clears the queue.
    /// </summary>
    public void Clear() => _innerQueue.Clear();

    public bool Contains(TItem item) => _innerQueue.Contains(item);

    public IEnumerator<TItem> GetEnumerator() => _innerQueue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _innerQueue.Count;
}