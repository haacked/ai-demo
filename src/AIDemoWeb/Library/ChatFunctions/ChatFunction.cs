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
    protected abstract string Name { get; }

    protected abstract string Description { get; }

    public FunctionDefinition Definition => new()
    {
        Name = Name,
        Description = Description,
        Parameters = BinaryDataGenerator.GenerateBinaryData(typeof(TArguments)),
    };

    async Task<string?> IChatFunction.InvokeAsync(
        string unvalidatedArguments,
        string source,
        CancellationToken cancellationToken)
    {
        var arguments = JsonSerializer.Deserialize<TArguments>(unvalidatedArguments, JsonSerialization.Options);

        if (arguments is null)
        {
            throw new InvalidOperationException(
                $"Could not deserialize arguments: `{unvalidatedArguments}` into {typeof(TArguments).Name}.");
        }

        // Actually call the weather service API.
        var result = await InvokeAsync(arguments, source, cancellationToken);

        if (result is null)
        {
            return null;
        }

        // Serialize the result data from the function into a new chat message with the 'Function' role,
        // then add it to the messages after the first User message and initial response FunctionCall
        return JsonSerializer.Serialize(
            result,
            JsonSerialization.Options);
    }

    protected abstract Task<TResult?> InvokeAsync(
        TArguments arguments,
        string source,
        CancellationToken cancellationToken);
}