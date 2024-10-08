using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using Serious;
using StackExchange.Redis;

namespace AIDemo.Library.Clients;

public class ChatHistoryCache(
    IConnectionMultiplexer connectionMultiplexer,
    string systemPrompt,
    string userIdentifier)
{
    readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    public async Task<ChatHistory> GetChatHistoryAsync()
    {
        // For now, we just store the entire history as one entry in Redis.
        // We may want to consider storing each message separately in the future.
        // That could make expunging older messages easier.
        var redisValue = await _db.StringGetAsync(userIdentifier);
        var history = redisValue != RedisValue.Null && redisValue.HasValue
            ? JsonSerializer.Deserialize<ChatHistory>(redisValue.ToString()).Require()
            : new ChatHistory(systemPrompt);

        if (history.Count is 1)
        {
            history.AddAssistantMessage("How may I help you?");
        }

        return history;
    }

    public async Task SaveChatHistoryAsync(ChatHistory chatHistory)
    {
        await _db.StringSetAsync(userIdentifier, JsonSerializer.Serialize(chatHistory));
    }

    public async Task DeleteChatHistoryAsync()
    {
        await _db.StringGetDeleteAsync(userIdentifier);
    }
}