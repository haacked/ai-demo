using Azure.AI.OpenAI;

namespace Serious.ChatFunctions;

public interface IChatFunction
{
    /// <summary>
    /// Describes the chat function.
    /// </summary>
    FunctionDefinition Definition { get; }

    /// <summary>
    /// Invokes the function and returns a JSON string with the result.
    /// </summary>
    /// <param name="unvalidatedArguments">The JSON arguments for the function as extracted by Chat GPT.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns></returns>
    Task<string?> InvokeAsync(string unvalidatedArguments, CancellationToken cancellationToken);
}
