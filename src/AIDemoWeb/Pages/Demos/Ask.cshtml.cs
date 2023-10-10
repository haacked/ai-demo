using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class AskPageModel : PageModel
{
    readonly OpenAIClientAccessor _client;

    public AskPageModel(OpenAIClientAccessor clientAccessor)
    {
        _client = clientAccessor;
    }

    [BindProperty]
    [Required]
    public string? Question { get; init; }

    [BindProperty]
    public string SystemPrompt { get; init; } =
        "Hello, you are a friendly chat bot who is part of a demo I'm giving and wants to represent me and Chat GPT well.";

    public string Answer { get; private set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var options = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System,  SystemPrompt),
                new ChatMessage(ChatRole.User, Question),
            },
            // Available functions.
            Functions = new List<FunctionDefinition>
            {
                new()
                {
                   Name = "arithmetic",
                   Description = "Performs simple arithmetic operations such as add, multiply, divide, subtract and so on.",
                   Parameters = BinaryData.FromObjectAsJson(
                       new
                       {
                           type = "object",
                           properties = new
                           {
                               left = new
                               {
                                   type = "number",
                                   description = "The value that serves as the left operand of the operation."
                               },
                               operation = new
                               {
                                   type = "string",
                                   description = "the operation to perform on the two operand values."
                               },
                               right = new
                               {
                                   type = "number",
                                   description = "The value that serves as the right operand of the operation."
                               }
                           },
                           required = new[] { "left", "op", "right" }
                       },
                       new JsonSerializerOptions
                       {
                           PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                           Converters = { new JsonStringEnumConverter() }
                       })
                }
            }
        };
        var response = await _client.GetChatCompletionsAsync(options, cancellationToken);

        Answer = response switch
        {
            { Value.Choices: [{ Message: { FunctionCall: { Name: "arithmetic" } } message}] }
                => await CallFunctionAsync(message),
            { HasValue: true } => string.Join("\n", response.Value.Choices.Select(c => c.Message.Content)),
            _ => "I don't know",
        };

        return Page();

        async Task<string> CallFunctionAsync(ChatMessage message)
        {
            var functionCall = message.FunctionCall;
            var arguments = JsonSerializer.Deserialize<ArithmeticArguments>(functionCall.Arguments, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            });

            if (arguments is null)
            {
                return "I don't know";
            }

            var result = arguments switch
            {
                { Operation: Operation.Add } => arguments.Left + arguments.Right,
                { Operation: Operation.Subtract } => arguments.Left - arguments.Right,
                { Operation: Operation.Multiply } => arguments.Left * arguments.Right,
                { Operation: Operation.Divide } => arguments.Left / arguments.Right,
                _ => throw new InvalidOperationException("Unknown operation.")
            };

            options.Messages.Add(message);

            var resultJson = JsonSerializer.Serialize(arguments with { Answer = result });
            options.Messages.Add(new ChatMessage(ChatRole.Function, resultJson)
            {
                Name = functionCall.Name,
            });

            var newResponse = await _client.GetChatCompletionsAsync(options, cancellationToken);

            return string.Join("\n", newResponse.Value.Choices.Select(c => c.Message.Content));
        }
    }
}

public record ArithmeticArguments(long Left, Operation Operation, long Right, long? Answer = null);


public enum Operation
{
    Add,
    Subtract,
    Multiply,
    Divide,
}