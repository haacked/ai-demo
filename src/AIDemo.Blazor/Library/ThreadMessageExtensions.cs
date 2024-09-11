using AIDemo.Library.Clients;
using OpenAI.Assistants;

namespace AIDemo.Library.Clients;

public static class ThreadMessageExtensions
{
    public static IEnumerable<BlazorMessage> ToBlazorMessages(this ThreadMessage message)
    {
        foreach (var content in message.Content)
        {
            var text = content switch
            {
                { Text: { } messageText } => messageText,
                { ImageFileId: { } imageFile } => $"File: {imageFile}",
                _ => "Unknown response",
            };

            var annotations = content.TextAnnotations ?? Array.Empty<TextAnnotation>();

            yield return new BlazorMessage(
                text,
                message.Role is MessageRole.User,
                message.CreatedAt.DateTime,
                annotations);
        }
    }
}