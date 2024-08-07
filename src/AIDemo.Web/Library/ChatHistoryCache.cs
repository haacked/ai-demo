using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serious;
using StackExchange.Redis;

namespace Haack.AIDemoWeb.Library;

public class ChatHistoryCache(
    IConnectionMultiplexer connectionMultiplexer,
    string prompt,
    string userKey)
{
    readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    public async Task<ChatHistory> GetChatHistoryAsync()
    {
        // For now, we just store the entire history as one entry in Redis.
        // We may want to consider storing each message separately in the future.
        // That could make expunging older messages easier.
        var redisValue = await _db.StringGetAsync(userKey);
        var promptMessage = new ChatMessageContent(AuthorRole.System, prompt);
        return redisValue != RedisValue.Null && redisValue.HasValue
            ? JsonSerializer.Deserialize<ChatHistory>(redisValue.ToString()).Require()
            : [promptMessage];
    }

    public async Task SaveChatHistoryAsync(ChatHistory chatHistory)
    {
        await _db.StringSetAsync(userKey, JsonSerializer.Serialize(chatHistory));
    }

    public async Task DeleteChatHistoryAsync()
    {
        await _db.StringGetDeleteAsync(userKey);
    }
}