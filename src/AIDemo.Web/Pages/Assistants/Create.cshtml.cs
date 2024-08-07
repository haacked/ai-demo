using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Refit;
using Serious;
using Serious.ChatFunctions;

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

    public IReadOnlyList<ToolDescriptor> AvailableTools { get; }

    public IReadOnlyList<FunctionToolDescriptor> AvailableFunctions { get; } = Array.Empty<FunctionToolDescriptor>();

    [TempData]
    public string? StatusMessage { get; set; }

    static readonly ToolDescriptor[] DefaultTools =
    {
        new("code_interpreter") { Description = "Code Interpreter" },
        new("retrieval") { Description = "Information Retrieval"},
    };

    public CreateAssistantPageModel(
        IOptions<OpenAIOptions> options,
        IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
        AvailableTools = DefaultTools;
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
            Tools = Tools.Select(GetToolByTypeOrName).ToList(),
        };
        try
        {
            var response = await _openAIClient.CreateAssistantAsync(
                _options.ApiKey.Require(),
                request,
                cancellationToken);

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

    AssistantTool GetToolByTypeOrName(string typeOrName)
    {
        var tool = AvailableTools.FirstOrDefault(t => t.Type == typeOrName);
        if (tool is not null)
        {
            return new AssistantTool(tool.Type);
        }

        var function = AvailableFunctions.FirstOrDefault(f => f.Function.Name == typeOrName);
        if (function is not null)
        {
            return new AssistantTool("function", function.Function);
        }

        throw new ArgumentException($"Unknown tool type or name: {typeOrName}");
    }
}

public record ToolDescriptor(string Type)
{
    public virtual string Description { get; init; } = string.Empty;
}

public record FunctionToolDescriptor(FunctionDescription Function) : ToolDescriptor("function")
{
    public override string Description => Function.Description;
}

public record FunctionDescription(string Name, string Description, IReadOnlyDictionary<string, object> Parameters);