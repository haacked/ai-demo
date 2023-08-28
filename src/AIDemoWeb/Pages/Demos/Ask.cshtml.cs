using System.ComponentModel.DataAnnotations;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;

namespace AIDemoWeb.Demos.Pages;

public class AskPageModel : PageModel
{
    readonly OpenAIOptions _options;

    public AskPageModel(IOptions<OpenAIOptions> options)
    {
        _options = options.Value;
    }

    [BindProperty]
    [Required]
    public string? Question { get; init; }

    public string Answer { get; private set; } = string.Empty;

    [BindProperty]
    [Required]
    public string? OpenAIModel { get; init; }

    public IReadOnlyList<SelectListItem> Models { get; private set; } = Array.Empty<SelectListItem>();

    public async Task OnGetAsync()
    {
        await InitializePageAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = await InitializePageAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }
        var chat = client.Chat.CreateConversation(new ChatRequest
        {
            Model = OpenAIModel
        });
        chat.AppendSystemMessage("Hello, you are a friendly chat bot who is part of a demo I'm giving and wants to represent me and Chat GPT well.");
        chat.AppendUserInput(Question);
        Answer = await chat.GetResponseFromChatbotAsync();

        return Page();
    }

    async Task<OpenAIAPI> InitializePageAsync()
    {
        var client = new OpenAIAPI(new APIAuthentication(_options.ApiKey, _options.OrganizationId));
        var models = await client.Models.GetModelsAsync();
        Models = models
            .Select(m => new SelectListItem(m.ModelID, m.ModelID))
            .ToList();
        return client;
    }
}
