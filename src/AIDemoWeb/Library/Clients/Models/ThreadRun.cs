using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Haack.AIDemoWeb.Library.Clients;

/// <summary>
/// Represents an execution run on a thread.
/// </summary>
/// <param name="Id">The identifier, which can be referenced in API endpoints.</param>
/// <param name="ObjectType">The object type, which is always <c>thread.run</c></param>
/// <param name="CreatedAt">The Unix timestamp (in seconds) for when the run was created.</param>
/// <param name="AssistantId">The ID of the assistant used for execution of this run.</param>
/// <param name="ThreadId">The ID of the thread that was executed on as a part of this run.</param>
/// <param name="Status">The status of the run, which can be either queued, in_progress, requires_action, cancelling, cancelled, failed, completed, or expired.</param>
/// <param name="StartedAt">The Unix timestamp (in seconds) for when the run was started.</param>
/// <param name="ExpiresAt">The Unix timestamp (in seconds) for when the run will expire.</param>
/// <param name="CancelledAt">The Unix timestamp (in seconds) for when the run was cancelled.</param>
/// <param name="FailedAt">The Unix timestamp (in seconds) for when the run failed.</param>
/// <param name="CompletedAt">The Unix timestamp (in seconds) for when the run was completed.</param>
/// <param name="LastError">The last error associated with this run. Will be null if there are no errors.</param>
/// <param name="Model">The model that the assistant used for this run.</param>
/// <param name="Instructions">The instructions that the assistant used for this run.</param>
/// <param name="Tools">The list of tools that the assistant used for this run.</param>
/// <param name="FileIds">The list of File IDs the assistant used for this run.</param>
/// <summary>
/// Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional
/// information about the object in a structured format. Keys can be a maximum of 64 characters long and values can
/// be a maximum of 512 characters long.
/// </summary>
public record ThreadRun(
    string Id,
    [property: JsonPropertyName("object")]
    string ObjectType,
    long CreatedAt,
    string AssistantId,
    string ThreadId,
    RunStatus Status,
    long StartedAt,
    long? ExpiresAt,
    long? CancelledAt,
    long? FailedAt,
    long? CompletedAt,
    OpenAIError LastError,
    string Model,
    string Instructions,
    IReadOnlyList<AssistantTool> Tools,
    IReadOnlyList<string> FileIds,
    IReadOnlyDictionary<string, string> Metadata
);

/// <summary>
/// An error associated with a run.
/// </summary>
/// <param name="Code">One of <c>server_error</c> or <c>rate_limit_exceeded</c>.</param>
/// <param name="Message"></param>
public record OpenAIError(string Code, string Message);

/// <summary>
/// Possible run statuses.
/// </summary>
/// <remarks>
/// The status of the run, which can be either queued, in_progress, requires_action, cancelling, cancelled, failed, completed, or expired.
/// </remarks>
public enum RunStatus
{
    [EnumMember(Value = "queued")]
    Queued,

    [EnumMember(Value = "in_progress")]
    InProgress,

    [EnumMember(Value = "requires_action")]
    RequiresAction,

    [EnumMember(Value = "cancelling")]
    Cancelling,

    [EnumMember(Value = "cancelled")]
    Cancelled,

    [EnumMember(Value = "failed")]
    Failed,

    [EnumMember(Value = "completed")]
    Completed,

    [EnumMember(Value = "expired")]
    Expired
}