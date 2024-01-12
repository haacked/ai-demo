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

    public HomePageModel(IOptions<GitHubOptions> githubOptions)
    {
        _gitHubOptions = githubOptions.Value;
    }

    public IActionResult OnGet()
    {
        // This is just here to make it easier to test the site locally.
        if (_gitHubOptions.Host is not null && Request.Host.Host != _gitHubOptions.Host)
        {
            return Redirect($"https://{_gitHubOptions.Host}/");
        }

        return Page();
    }
}