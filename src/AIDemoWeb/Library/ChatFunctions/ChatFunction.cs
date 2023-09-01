using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;

namespace Serious.ChatFunctions;

/// <summary>
/// Base class for chat functions which are passed their strongly typed arguments.
/// </summary>
/// <typeparam name="TArguments">The arguments type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class ChatFunction<TArguments, TResult> : IChatFunction where TArguments : class
{
    public abstract FunctionDefinition Definition { get; }

    async Task<string?> IChatFunction.InvokeAsync(string unvalidatedArguments, CancellationToken cancellationToken)
    {
        var arguments = JsonSerializer.Deserialize<TArguments>(unvalidatedArguments, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        }).Require();
        // Actually call the weather service API.
        var result = await InvokeAsync(arguments, cancellationToken);

        // Serialize the result data from the function into a new chat message with the 'Function' role,
        // then add it to the messages after the first User message and initial response FunctionCall
        return JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    protected abstract Task<TResult?> InvokeAsync(TArguments arguments, CancellationToken cancellationToken);
}