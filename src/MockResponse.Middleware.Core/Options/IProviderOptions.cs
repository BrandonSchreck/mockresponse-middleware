namespace MockResponse.Middleware.Core.Options;

/// <summary>
/// Enforces each provider options type declares the default name of its corresponding JSON configuration section.
/// </summary>
public interface IProviderOptions
{
    /// <summary>
    /// The name of the JSON subsection under <see cref="MockOptions"/> that should be used to bind configuration values.
    /// </summary>
    static abstract string SectionName { get; }
}