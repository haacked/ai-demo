namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A thread with an Open AI assistant. There's no API to list threads, so we want to track them ourselves.
/// </summary>
/// <remarks>
/// The approach to using an assistant is to create a thread, add messages to it, and then create a run when we want
/// the assistant to interact with the user. It's possible to list a thread's runs via the api.
/// </remarks>
public class AssistantThread
{
    public int Id { get; init; }

    /// <summary>
    /// The Open AI thread identifier (not the same as the database id). Example: thread_abc123
    /// </summary>
    public required string ThreadId { get; init; }

    /// <summary>
    /// The app user that created this thread.
    /// </summary>
    public required User Creator { get; init; }

    /// <summary>
    /// The Id of the app user that created this thread.
    /// </summary>
    public required int CreatorId { get; init; }
}