using Azure;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;
using Pgvector;

namespace Serious;

/// <summary>
/// Used to access a configured <see cref="OpenAIClient"/> instance.
/// </summary>
public class OpenAIClientAccessor
{
    readonly IOpenAIClient _openAIClient;
    readonly OpenAIOptions _options;

    public OpenAIClientAccessor(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
        _options = options.Value;

        Client = new OpenAIClient(_options.ApiKey);
    }

    /// <summary>
    /// Get chat completions for provided chat context messages.
    /// </summary>
    /// <param name="chatCompletionsOptions">The options for this chat completions request.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    public async Task<Response<ChatCompletions>> GetChatCompletionsAsync(
        ChatCompletionsOptions chatCompletionsOptions,
        CancellationToken cancellationToken = default) =>
        await Client.GetChatCompletionsAsync(
            new ChatCompletionsOptions(_options.Model, chatCompletionsOptions.Messages)
            {
                // HACK: This is ugly. I have to copy everything.
                // I filed an issue and am crossing my fingers: https://github.com/Azure/azure-sdk-for-net/issues/40002
                Functions = chatCompletionsOptions.Functions,
            },
            cancellationToken);

    /// <summary>
    /// Return the computed embeddings for a given prompt.
    /// </summary>
    /// <param name="embeddingsOptions">The options for this embeddings request.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    public async Task<Response<Embeddings>> GetEmbeddingsAsync(
        EmbeddingsOptions embeddingsOptions,
        CancellationToken cancellationToken = default) =>
        await Client.GetEmbeddingsAsync(
            new EmbeddingsOptions(_options.EmbeddingModel, embeddingsOptions.Input),
            cancellationToken);


    /// <summary>
    /// Return the computed embeddings for a given prompt.
    /// </summary>
    /// <param name="prompt">The prompt for this embeddings request.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    public async Task<Vector?> GetEmbeddingsAsync(string prompt, CancellationToken cancellationToken)
    {
        var response = await GetEmbeddingsAsync(
            new EmbeddingsOptions { Input = [ prompt ]},
            cancellationToken);
        if (response.HasValue)
        {
            var embedding = response.Value.Data;
            if (embedding is { Count: > 0 })
            {
                return new Vector(embedding[0].Embedding.ToArray());
            }
        }

        return null;
    }

    public async Task<List<OpenAIEntity>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _openAIClient.GetModelsAsync(_options.ApiKey.Require(), cancellationToken);
        return result.Data;
    }

    public OpenAIClient Client { get; }
}

