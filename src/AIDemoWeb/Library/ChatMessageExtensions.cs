using System.Text.Json;
using Azure.AI.OpenAI;

namespace Haack.AIDemoWeb.Library;

public static class ChatMessageExtensions
{
    public static IEnumerable<ChatMessage> FromSerializedJson(string? json)
    {
        if (json is null)
        {
            return Array.Empty<ChatMessage>();
        }
        return (JsonSerializer.Deserialize<SerializedChatMessage[]>(json) ?? Array.Empty<SerializedChatMessage>())
            .Select(m => m.FromSerializedChatMessage())
            .ToList();
    }

    public static string ToJson(this IEnumerable<ChatMessage> chatMessages)
    {
        return JsonSerializer.Serialize(chatMessages.Select(m => m.ToSerializedChatMessage()));
    }

    static SerializedChatMessage ToSerializedChatMessage(this ChatMessage message)
    {
        return new SerializedChatMessage(
            message.Role.ToString() ?? throw new InvalidOperationException($"Unknown role {message.Role}"),
            message.Content,
            message.Name,
            message.FunctionCall);
    }

    static ChatMessage FromSerializedChatMessage(this SerializedChatMessage message)
    {
        return new ChatMessage(message.Role, message.Content)
        {
            Name = message.Name,
            FunctionCall = message.FunctionCall,
        };
    }
}

public record SerializedChatMessage(
    string Role,
    string Content,
    string? Name = null,
    FunctionCall? FunctionCall = null);