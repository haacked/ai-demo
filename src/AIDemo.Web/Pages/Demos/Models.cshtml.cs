using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Models;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ModelsPageModel(OpenAIClient openAIClient, IOptions<OpenAIOptions> options)
    : PageModel
{
    public string CompletionsModel { get; } = options.Value.Require().Model;

    public string EmbeddingsModel { get; } = options.Value.Require().EmbeddingModel;

    public IReadOnlyList<OpenAIModelInfo> Models { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        var modelClient = openAIClient.GetModelClient();
        Models = (await modelClient.GetModelsAsync()).Value;
    }
}
