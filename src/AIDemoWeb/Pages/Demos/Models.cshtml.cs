using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ModelsPageModel(OpenAIClientAccessor clientAccessor, IOptions<OpenAIOptions> options)
    : PageModel
{
    public string CompletionsModel { get; } = options.Value.Require().Model;

    public string EmbeddingsModel { get; } = options.Value.Require().EmbeddingModel;

    public IReadOnlyList<OpenAIEntity> Models { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Models = await clientAccessor.GetModelsAsync();
    }
}
