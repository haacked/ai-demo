using Azure.AI.OpenAI;

namespace Serious.ChatFunctions;

public interface IChatFunction
{
    int Order => 0;

    /// <summary>
    /// Describes the chat function.
    /// </summary>
    FunctionDefinition Definition { get; }

    /// <summary>
    /// Invokes the function and returns a JSON string with the result.
    /// </summary>
    /// <param name="unvalidatedArguments">The JSON arguments for the function as extracted by Chat GPT.</param>
    /// <param name="source">The source message that caused the function to be invoked.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<string?> InvokeAsync(string unvalidatedArguments, string source, CancellationToken cancellationToken);
}
