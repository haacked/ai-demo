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

    /// <summary>
    /// Creates an assistant.
    /// </summary>
    /// <param name="apiToken">The Open AI API Key.</param>
    /// <param name="body">The assistant to create.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The created assistant.</returns>
    [Post("/assistants")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<Assistant> CreateAssistantAsync(
        [Authorize]
        string apiToken,
        AssistantCreateBody body,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes an assistant.
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="assistantId">The ID of the assistant to delete.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The result of the operation.</returns>
    [Delete("/assistants/{assistantId}")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<ObjectDeletedResponse> DeleteAssistantAsync(
        [Authorize]
        string apiToken,
        string assistantId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a list of files.
    /// </summary>
    /// <param name="apiToken">The Open AI API Key.</param>
    /// <param name="purpose">Only returns files with the given purpose.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A list of files.</returns>
    [Get("/files")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<OpenAIResponse<List<File>>> GetFilesAsync(
        [Authorize]
        string apiToken,
        string? purpose = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a file to the Open AI API.
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="purpose">The intended purpose of the uploaded file. Can be 'fine-tune' or 'assistants'.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns></returns>
    [Multipart]
    [Post("/files")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<File> UploadFileAsync(
        [Authorize]
        string apiToken,
        string purpose,
        StreamPart file,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="fileId">The ID of the file to delete.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The result of the operation.</returns>
    [Delete("/files/{fileId}")]
    [Headers("OpenAI-Beta: assistants=v1")]
    Task<ObjectDeletedResponse> DeleteFileAsync(
        [Authorize]
        string apiToken,
        string fileId,
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
/// <param name="Type">The type of tool. Either <c>code_interpreter</c>, <c>function</c>, or <c>retrieval</c></param>
public record AssistantTool(string Type);