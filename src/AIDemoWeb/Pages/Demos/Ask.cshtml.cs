using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Demos.Pages;

public class AskPageModel(OpenAIClientAccessor clientAccessor) : PageModel
{
    [BindProperty]
    [Required]
    public string? Question { get; init; }

    [BindProperty]
    public string? PreviousMessages { get; set; }

    [BindProperty]
    public string SystemPrompt { get; init; } =
        "Hello, you are a friendly chat bot who is part of a demo I'm giving and wants to represent me and Chat GPT well.";

    public string Answer { get; private set; } = string.Empty;

    static readonly string[] RequiredArguments = { "left", "op", "right" };

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
                new ChatMessage(ChatRole.System, SystemPrompt)
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
                           required = RequiredArguments,
                       },
                       new JsonSerializerOptions
                       {
                           PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                           Converters = { new JsonStringEnumConverter() }
                       })
                }
            }
        };

        foreach (var message in ChatMessageExtensions.FromSerializedJson(PreviousMessages))
        {
            options.Messages.Add(message);
        }
        options.Messages.Add(new ChatMessage(ChatRole.User, Question));

        var response = await clientAccessor.GetChatCompletionsAsync(options, cancellationToken);

        Answer = response switch
        {
            { Value.Choices: [{ Message: { FunctionCall.Name: "arithmetic" } message }] }
                => await CallFunctionAsync(message),
            { HasValue: true } => ReturnAnswerAsync(response.Value.Choices[0]),
            _ => "I don't know",
        };

        // Serialize the new set of "previous messages into the hidden input"
        PreviousMessages = options.Messages.Skip(1).ToJson(); // Skip the system message.

        return Page();

        string ReturnAnswerAsync(ChatChoice choice)
        {
            options.Messages.Add(choice.Message);

            return choice.Message.Content;
        }

        async Task<string> CallFunctionAsync(ChatMessage message)
        {
            var functionCall = message.FunctionCall;
            var arguments = JsonSerializer.Deserialize<ArithmeticArguments>(
                functionCall.Arguments,
                JsonSerialization.Options);

            if (arguments is null)
            {
                return "I don't know";
            }

            var result = MathExtensions.DoArithmetic(arguments);

            options.Messages.Add(message);

            var resultJson = JsonSerializer.Serialize(arguments with { Answer = result });
            options.Messages.Add(new ChatMessage(ChatRole.Function, resultJson)
            {
                Name = functionCall.Name,
            });

            var newResponse = await clientAccessor.GetChatCompletionsAsync(options, cancellationToken);

            var assistantMessage = newResponse.Value.Choices[0].Message;
            options.Messages.Add(assistantMessage);
            return assistantMessage.Content;
        }
    }
}
