using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Refit;
using Serious;

#pragma warning disable CA2227, CA1002, CA1819

namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateAssistantPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    [BindProperty]
    public AssistantCreateBody Assistant { get; init; } = null!;

    [BindProperty]
    public string[] FileIds { get; init; } = Array.Empty<string>();

    [BindProperty]
    public string[] Tools { get; init; } = Array.Empty<string>();

    public IReadOnlyList<SelectListItem> AvailableFileIds { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<string> AvailableTools { get; set; } = new [] { "code_interpreter", "retrieval" };

    [TempData]
    public string? StatusMessage { get; set; }

    public CreateAssistantPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.GetFilesAsync(_options.ApiKey.Require(), "assistants", cancellationToken);
        AvailableFileIds = response.Data.Select(f => new SelectListItem(f.Filename, f.Id)).ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = Assistant with
        {
            Model = _options.RetrievalModel,
            FileIds = FileIds,
            Tools = Tools.Select(t => new AssistantTool(t)).ToList(),
        };
        try
        {
            var response =
                await _openAIClient.CreateAssistantAsync(_options.ApiKey.Require(), request, cancellationToken);

            StatusMessage = $"Assistant {response.Id} created.";

            // TODO: Associate the Id of the assistant with the current user so no other fools can use it.
            return RedirectToPage("Index");
        }
        catch (ApiException e)
        {
            ModelState.AddModelError("", e.Content ?? e.Message);
            return Page();
        }
    }
}