namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// This app uses GitHub to log in. This is used to configure the GitHub OAuth app.
/// </summary>
public class GitHubOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string GitHub = "GitHub";

    /// <summary>
    /// The GitHub app host.
    /// </summary>
    public string? Host { get; init; }
}