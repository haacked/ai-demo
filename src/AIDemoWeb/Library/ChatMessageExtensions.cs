using Azure.AI.OpenAI;

namespace Haack.AIDemoWeb.Library;

public static class ChatMessageExtensions
{
    public static ChatRequestMessage ToChatRequestMessage(this ChatResponseMessage responseMessage)
    {
        if (responseMessage.Role == ChatRole.System)
        {
            return new ChatRequestSystemMessage(responseMessage.Content);
        }
        if (responseMessage.Role == ChatRole.Assistant)
        {
            var assistantMessage = new ChatRequestAssistantMessage(responseMessage.Content);
            assistantMessage.ToolCalls.AddRange(responseMessage.ToolCalls);
            assistantMessage.FunctionCall = responseMessage.FunctionCall;
            return assistantMessage;
        }
        if (responseMessage.Role == ChatRole.User)
        {
            return new ChatRequestUserMessage(responseMessage.Content);
        }
        if (responseMessage.Role == ChatRole.Function)
        {
            return new ChatRequestFunctionMessage(responseMessage.FunctionCall.Name, responseMessage.Content);
        }

        throw new InvalidOperationException($"Unexpected role `{responseMessage.Role}` in response message.");
    }
}

public record SerializedChatMessage(
    string Role,
    string Content,
    string? Name = null,
    FunctionCall? FunctionCall = null);