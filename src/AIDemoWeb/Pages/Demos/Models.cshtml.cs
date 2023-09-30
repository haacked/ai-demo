using System.ComponentModel.DataAnnotations;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ModelsPageModel : PageModel
{
    readonly OpenAIClientAccessor _client;

    public ModelsPageModel(OpenAIClientAccessor clientAccessor, IOptions<OpenAIOptions> options)
    {
        _client = clientAccessor;
        CompletionsModel = options.Value.Require().Model;
        EmbeddingsModel = options.Value.Require().EmbeddingModel;
    }

    public string CompletionsModel { get; }

    public string EmbeddingsModel { get; }

    public IReadOnlyList<OpenAIEntity> Models { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Models = await _client.GetModelsAsync();
    }
}
