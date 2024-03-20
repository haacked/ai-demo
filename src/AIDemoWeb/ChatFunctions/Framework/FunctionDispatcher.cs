using System.Reflection;
using System.Text.Json;
using AIDemoWeb;
using Azure.AI.OpenAI;

namespace Serious.ChatFunctions;

/// <summary>
/// Class used to dispatch Chat GPT function calls to the appropriate <see cref="IChatFunction"/>.
/// </summary>
public class FunctionDispatcher
{
    readonly Dictionary<string, IChatFunction> _functions;

    public FunctionDispatcher(IEnumerable<IChatFunction> functions)
    {
        _functions = functions.ToDictionary(fun => fun.Definition.Name);
    }

    /// <summary>
    /// Dispatches a function call to the appropriate function and returns the embedded as a
    /// <see cref="ChatRequestMessage"/> with the <see cref="ChatRole"/> of <see cref="ChatRole.Function"/>. This
    /// message can be passed back to GPT to be summarized and returned to the user.
    /// </summary>
    /// <param name="functionCall">A <see cref="FunctionCall"/> as returned by Chat GPT.</param>
    /// <param name="source">The source message that caused the function to be invoked.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task<ChatRequestFunctionMessage?> DispatchAsync(
        FunctionCall functionCall,
        string source,
        CancellationToken cancellationToken)
    {
        if (!_functions.TryGetValue(functionCall.Name, out var function))
        {
            return null;
        }

        var result = await function.InvokeAsync(functionCall.Arguments, source, cancellationToken);

        if (result is null)
        {
            return null;
        }

        // Serialize the result data from the function into a new chat message with the 'Function' role,
        // then add it to the messages after the first User message and initial response FunctionCall
        var content = JsonSerializer.Serialize(
            result,
            JsonSerialization.Options);

        return new ChatRequestFunctionMessage(functionCall.Name, content);
    }

    /// <summary>
    /// Returns a list of function definitions.
    /// </summary>
    public IList<FunctionDefinition> GetFunctionDefinitions() => _functions
        .Values
        .OrderBy(f => f.Order)
        .Select(f => f.Definition)
        .Select(d => new FunctionDefinition
        {
            Name = d.Name,
            Description = d.Description,
            Parameters = d.Parameters.ToBinaryData(),
        })
        .ToList();
}

public static class FunctionDispatcherServiceExtensions
{
    /// <summary>
    /// Registers the function dispatchers and all <see cref="IChatFunction"/> in the specified assembly or assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to search.</param>
    public static void AddFunctionDispatcher(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddScoped<FunctionDispatcher>();
        services.RegisterAllTypes<IChatFunction>(ServiceLifetime.Scoped, publicOnly: true, assemblies);
    }
}