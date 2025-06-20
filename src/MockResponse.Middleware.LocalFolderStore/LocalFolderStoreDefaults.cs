using MockResponse.Middleware.Core.Options;

namespace MockResponse.Middleware.LocalFolderStore;

/// <summary>
/// Contains default values for the LocalFolderStore mock provider.
/// </summary>
internal static class LocalFolderStoreDefaults
{
    /// <summary>
    /// The default name used to identify the LocalFolderStore provider.
    /// This value is used as the subsection name under <see cref="MockOptions"/> for binding
    /// <see cref="LocalFolderStoreOptions"/>.
    /// </summary>
    public const string Name = "LocalFolderStore";
}