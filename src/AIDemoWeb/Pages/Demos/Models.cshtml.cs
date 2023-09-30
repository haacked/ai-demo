using System.ComponentModel.DataAnnotations;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ModelsPageModel : PageModel
{
    readonly OpenAIClientAccessor _client;

    public ModelsPageModel(OpenAIClientAccessor clientAccessor)
    {
        _client = clientAccessor;
    }

    public IReadOnlyList<OpenAIEntity> Models { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Models = await _client.GetModelsAsync();
    }
}
