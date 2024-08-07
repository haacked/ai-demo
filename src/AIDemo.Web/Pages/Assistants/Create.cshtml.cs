using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Assistants;
using Refit;

#pragma warning disable CA2227, CA1002, CA1819, OPENAI001


namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateAssistantPageModel(IOptions<OpenAIOptions> options, OpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    [BindProperty]
    public AssistantCreationOptions Assistant { get; init; } = null!;

    [BindProperty]
    public string[] FileIds { get; init; } = [];

    [BindProperty]
    public string[] Tools { get; init; } = [];

    public IReadOnlyList<SelectListItem> AvailableFileIds { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<ToolDescriptor> AvailableTools { get; } = DefaultTools;

    public IReadOnlyList<FunctionToolDescriptor> AvailableFunctions { get; } = Array.Empty<FunctionToolDescriptor>();

    [TempData]
    public string? StatusMessage { get; set; }

    static readonly ToolDescriptor[] DefaultTools =
    {
        new("code_interpreter") { Description = "Code Interpreter" },
        new("retrieval") { Description = "Information Retrieval"},
    };

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var fileClient = openAIClient.GetFileClient();

        var response = await fileClient.GetFilesAsync("assistants", cancellationToken);
        AvailableFileIds = response.Value.Select(f => new SelectListItem(f.Filename, f.Id)).ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var assistant = openAIClient.GetAssistantClient();

        try
        {
            var response = await assistant.CreateAssistantAsync(
                _options.Model,
                Assistant,
                cancellationToken);

            StatusMessage = $"Assistant {response.Value.Id} created.";

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

public record ToolDescriptor(string Type)
{
    public virtual string Description { get; init; } = string.Empty;
}

public record FunctionToolDescriptor(FunctionDescription Function) : ToolDescriptor("function")
{
    public override string Description => Function.Description;
}

public record FunctionDescription(string Name, string Description, IReadOnlyDictionary<string, object> Parameters);