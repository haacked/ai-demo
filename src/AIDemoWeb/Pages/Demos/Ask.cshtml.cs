﻿using System.ComponentModel.DataAnnotations;
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
            }
        };
        var response = await _client.GetChatCompletionsAsync(options, cancellationToken);

        Answer = string.Join("\n", response.Value.Choices.Select(c => c.Message.Content));

        return Page();
    }
}