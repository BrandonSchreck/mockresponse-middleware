using System.ComponentModel.DataAnnotations;
using MockResponse.Middleware.Core.Options;

namespace MockResponse.Middleware.LocalFolderStore;

/// <summary>
/// Configuration options for the LocalFolderStore mock-storage provider.
/// These options are bound from the <c>MockOptions:LocalFolderStoreOptions</c> section by default.
/// </summary>
public record LocalFolderStoreOptions : IProviderOptions
{
    /// <summary>
    /// The name of the configuration section under <see cref="MockOptions"/> to bind the provider's
    /// settings from. Used internally to resolve and bind <see cref="LocalFolderStoreOptions"/>.
    /// </summary>
    public static string SectionName => nameof(LocalFolderStoreOptions);

    /// <summary>
    /// The full relative path to the local directory that contains mock response files.
    /// This directory must exist and be accessible by the application at runtime.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string FolderPath { get; set; } = default!;
}