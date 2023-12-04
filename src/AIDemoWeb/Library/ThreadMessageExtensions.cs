using Haack.AIDemoWeb.Library.Clients;

namespace Haack.AIDemoWeb.Library;

public static class ThreadMessageExtensions
{
    public static IEnumerable<BlazorMessage> ToBlazorMessages(this ThreadMessage message)
    {
        foreach (var content in message.Content)
        {
            var text = content switch
            {
                { Text: { } messageText } => messageText.Value,
                { ImageFile: { } imageFile } => $"File: {imageFile.FileId}",
                _ => "Unknown response",
            };

            var annotations = content.Text?.Annotations ?? Array.Empty<Annotation>();

            yield return new BlazorMessage(
                text,
                message.Role is "user",
                DateTimeOffset.FromUnixTimeSeconds(message.CreatedAt).DateTime,
                annotations);
        }
    }
}