using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateAssistantPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    [BindProperty]
    public string Name { get; init; } = "";

    [TempData]
    public string? StatusMessage { get; set; }

    public CreateAssistantPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new AssistantCreateBody
        {
            Model = _options.Model,
            Name = Name,
        };
        var response = await _openAIClient.CreateAssistantAsync(_options.ApiKey.Require(), request, cancellationToken);


        StatusMessage = $"Assistant {response.Id} created.";

        // TODO: Associate the Id of the assistant with the current user so no other fools can use it.
        return RedirectToPage("Index");
    }
}