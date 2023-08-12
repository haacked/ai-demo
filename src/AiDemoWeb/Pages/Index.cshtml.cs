using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace AIDemoWeb.Pages;

public class IndexModel : PageModel
{
    readonly ILogger<IndexModel> _logger;
    readonly OpenAIOptions _options;

    public IndexModel(IOptions<OpenAIOptions> options, ILogger<IndexModel> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [BindProperty]
    public string? Question { get; init; }

    [BindProperty]
    public string? Model { get; init; }

    public IReadOnlyList<SelectListItem> Models { get; private set; } = Array.Empty<SelectListItem>();

    public async Task OnGetAsync()
    {
        var client = new OpenAIAPI(new APIAuthentication(_options.ApiKey, _options.OrganizationId));
        var models = await client.Models.GetModelsAsync();
        Models = models
            .Select(m => new SelectListItem(m.ModelID, m.ModelID))
            .ToList();
    }
}
