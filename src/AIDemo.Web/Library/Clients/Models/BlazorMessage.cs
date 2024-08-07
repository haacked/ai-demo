using OpenAI.Assistants;

namespace Haack.AIDemoWeb.Library.Clients;

public record BlazorMessage(
    string Text,
    bool IsUser,
    DateTime Created,
    IReadOnlyList<TextAnnotation>? Annotations = null);