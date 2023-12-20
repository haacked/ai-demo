using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AIDemoWeb.Pages;

[AllowAnonymous]
public class HomePageModel : PageModel
{
    readonly GitHubOptions _gitHubOptions;

    public HomePageModel(IOptions<GitHubOptions> gitHubOptions)
    {
        _gitHubOptions = gitHubOptions.Value;
    }

    public IActionResult OnGet()
    {
        if (_gitHubOptions.Host is not null && Request.Host.Host != _gitHubOptions.Host)
        {
            return Redirect($"https://{_gitHubOptions.Host}/");
        }
        return Page();
    }
}