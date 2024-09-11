using AIDemo.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI;
using OpenAI.Assistants;
using Serious;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class AssistantsIndexPageModel(OpenAIClient openAIClient) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? AssistantIdToDelete { get; set; }

    public IReadOnlyList<Assistant> Assistants { get; private set; } = Array.Empty<Assistant>();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001
        Assistants = await assistantClient
            .GetAssistantsAsync(cancellationToken: cancellationToken)
            .GetAllValuesAsync(cancellationToken)
            .ToReadOnlyListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001

        await assistantClient.DeleteAssistantAsync(AssistantIdToDelete.Require(), cancellationToken);
        StatusMessage = $"Assistant {AssistantIdToDelete} deleted.";
        return RedirectToPage();
    }
}