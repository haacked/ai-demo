using OpenAI.Assistants;

namespace AIDemo.Blazor.Library;

public record BlazorMessage(
    string Text,
    bool IsUser,
    DateTime Created,
    IReadOnlyList<TextAnnotation>? Annotations = null);