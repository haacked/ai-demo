namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// This app uses GitHub to log in. This is used to configure the GitHub OAuth app.
/// </summary>
public class GitHubOptions : IConfigOptions
{
    /// <summary>
    /// The GitHub app host.
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// The config section name. Leave null to use part of the name of this type before the "Options" suffix
    /// as the section name.
    /// </summary>
    public static string SectionName => "GitHub";
}