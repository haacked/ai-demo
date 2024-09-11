using OpenAI.Assistants;

namespace AIDemo.Library.Clients;

public record BlazorMessage(
    string Text,
    bool IsUser,
    DateTime Created,
    IReadOnlyList<TextAnnotation>? Annotations = null);