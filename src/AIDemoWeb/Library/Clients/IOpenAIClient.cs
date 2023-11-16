using System.Text.Json.Serialization;
using Refit;

namespace Haack.AIDemoWeb.Library.Clients;

/// <summary>
/// Unfortunately, the Azure Open AI Client doesn't implement every endpoint. For example,
/// we want to be able to list models.
/// </summary>
public interface IOpenAIClient
{
    public static Uri BaseAddress => new("https://api.openai.com/v1");

    /// <summary>
    /// Returns a list of models.
    /// </summary>
    /// <param name="apiToken">The Open AI API Key.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A list of models.</returns>
    [Get("/models")]
    Task<OpenAIResponse<List<OpenAIEntity>>> GetModelsAsync(
        [Authorize] string apiToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of assistants.
    /// </summary>
    /// <param name="apiToken">The Open AI API Key.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A list of assistants.</returns>
    [Get("/assistants")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<OpenAIResponse<List<Assistant>>> GetAssistantsAsync(
        [Authorize]
        string apiToken,
        CancellationToken cancellationToken = default
    );
}

public record OpenAIResponse<T>(
    [property: JsonPropertyName("object")]
    string ObjectType,
    T Data);

/// <summary>
/// A model returned from the Open AI API.
/// </summary>
/// <param name="Id">The Id (or name) of the model.</param>
/// <param name="ObjectType">The type of object.</param>
/// <param name="Created">When the entity was created.</param>
/// <param name="OwnedBy">Who owns the entity.</param>
public record OpenAIEntity(
    string Id,

    [property: JsonPropertyName("object")]
    string ObjectType,

    long Created,

    string OwnedBy);

/// <summary>
/// A tool that is enabled for an assistant.
/// </summary>
/// <param name="Type"></param>
public record AssistantTool(string Type);