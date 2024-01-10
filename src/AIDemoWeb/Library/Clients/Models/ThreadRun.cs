using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;

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
/// <param name="Metadata">
/// Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional
/// information about the object in a structured format. Keys can be a maximum of 64 characters long and values can
/// be a maximum of 512 characters long.
/// </param>
/// <param name="RequiredAction">
/// Details on the action required to continue the run. Will be <c>null</c> if no action is required
/// </param>
public record ThreadRun(
    string Id,
    [property: JsonPropertyName("object")]
    string ObjectType,
    long CreatedAt,
    string AssistantId,
    string ThreadId,
    string Status,
    long? StartedAt,
    long? ExpiresAt,
    long? CancelledAt,
    long? FailedAt,
    long? CompletedAt,
    OpenAIError LastError,
    string Model,
    string Instructions,
    IReadOnlyList<AssistantTool> Tools,
    IReadOnlyList<string> FileIds,
    IReadOnlyDictionary<string, string> Metadata,
    RequiredAction? RequiredAction = null
);

/// <summary>
/// An error associated with a run.
/// </summary>
/// <param name="Code">One of <c>server_error</c> or <c>rate_limit_exceeded</c>.</param>
/// <param name="Message"></param>
public record OpenAIError(string Code, string Message);

/// <summary>
/// Used to create a thread run.
/// </summary>
public record ThreadRunCreateBody
{
    /// <summary>
    /// The ID of the assistant to use to execute this run.
    /// </summary>
    public required string AssistantId { get; init; }

    /// <summary>
    /// The ID of the Model to be used to execute this run. If a value is provided here, it will override the model
    /// associated with the assistant. If not, the model associated with the assistant will be used.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Override the default system message of the assistant. This is useful for modifying the behavior on a per-run
    /// basis.
    /// </summary>
    public string? Instructions { get; init; }

    /// <summary>
    /// Override the tools the assistant can use for this run. This is useful for modifying the behavior on a
    /// per-run basis.
    /// </summary>
    public IReadOnlyList<AssistantTool>? Tools { get; init; }

    /// <summary>
    /// Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional
    /// information about the object in a structured format. Keys can be a maximum of 64 characters long and values can
    /// be a maximum of 512 characters long.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// The body of the request to submit tool outputs.
/// </summary>
/// <param name="ToolOutputs">A list of tools for which the outputs are being submitted.</param>
public record ToolsOutputsSubmissionBody(IReadOnlyList<ToolOutput> ToolOutputs);

/// <summary>
/// Output of calling a tool (aka function).
/// </summary>
/// <param name="ToolCallId">
/// The ID of the tool call in the <c>required_action</c> object within the run object the output is being
/// submitted for.
/// </param>
/// <param name="Output">The output of the tool call to be submitted to continue the run.</param>
public record ToolOutput(string ToolCallId, string Output);

/// <summary>
/// Details on the action required to continue the run. Will be null if no action is required
/// </summary>
/// <param name="SubmitToolOutputs">Details on the tool outputs needed for this run to continue.</param>
public record RequiredAction(RequiredToolsOutputs SubmitToolOutputs)
{
    public string Type => "submit_tool_outputs";
}

/// <summary>
/// Details on the tool outputs needed for this run to continue.
/// </summary>
/// <param name="ToolCalls">A list of the relevant tool calls.</param>
public record RequiredToolsOutputs(IReadOnlyList<RequiredToolCall> ToolCalls);

/// <summary>
/// Information about a tool to call. We still need to do the calling and submit the output.
/// </summary>
/// <param name="Id">The ID of the tool call. This ID must be referenced when you submit the tool outputs in using the Submit tool outputs to run endpoint.</param>
/// <param name="Function">The function definition.</param>
public record RequiredToolCall(string Id, FunctionCall Function)
{
    /// <summary>
    /// The type of tool call the output is required for. For now, this is always <c>function</c>.
    /// </summary>
    public string Type => "function";
}

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