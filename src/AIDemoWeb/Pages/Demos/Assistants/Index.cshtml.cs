using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class AssistantsIndexPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    public IReadOnlyList<Assistant> Assistants { get; private set; } = Array.Empty<Assistant>();

    public AssistantsIndexPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.GetAssistantsAsync(_options.ApiKey.Require(), cancellationToken);
        Assistants = response.Data;
    }
}