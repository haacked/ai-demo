using Azure;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;

namespace Serious;

/// <summary>
/// Used to access a configured <see cref="OpenAIClient"/> instance.
/// </summary>
public class OpenAIClientAccessor
{
    readonly OpenAIOptions _options;

    public OpenAIClientAccessor(IOptions<OpenAIOptions> options)
    {
        _options = options.Value;

        Client = new OpenAIClient(_options.ApiKey);
    }

    public async Task<Response<ChatCompletions>> GetChatCompletionsAsync(
        ChatCompletionsOptions chatCompletionsOptions,
        CancellationToken cancellationToken = default) =>
        await Client.GetChatCompletionsAsync(
            _options.Model,
            chatCompletionsOptions,
            cancellationToken);

    public OpenAIClient Client { get; }
}