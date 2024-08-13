namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// Base interface for custom configuration sections.
/// </summary>
public interface IConfigOptions
{
    /// <summary>
    /// The name of the config section. If null, the part of the type name before "Options" is used
    /// as the section name by convention.
    /// </summary>
    static abstract string SectionName { get; }
}

