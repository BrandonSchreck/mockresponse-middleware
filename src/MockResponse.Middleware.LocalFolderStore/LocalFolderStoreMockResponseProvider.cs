using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.LocalFolderStore.Tests")]
namespace MockResponse.Middleware.LocalFolderStore;

/// <summary>
/// A mock response provider that reads JSON response files from a specified local folder on disk.
/// Implements dynamic configuration via <see cref="IOptionsMonitor{TOptions}"/> to support runtime updates.
/// </summary>
internal sealed class LocalFolderStoreMockResponseProvider : IMockResponseProvider, IMockResponseProviderDefinition
{
    /// <summary>
    /// Gets the logical name of the provider used for registration and logging.
    /// </summary>
    public string Name => ProviderName;

    /// <summary>
    /// The static name used to identify this provider.
    /// </summary>
    public static string ProviderName => LocalFolderStoreDefaults.Name;

    private string _folderPath = default!;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFolderStoreMockResponseProvider"/> class.
    /// </summary>
    /// <param name="fileSystem">An abstraction over file system access for easier testing.</param>
    /// <param name="options">An options monitor that provides configuration and supports change tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> or <paramref name="options"/> are null.</exception>
    public LocalFolderStoreMockResponseProvider(IFileSystem fileSystem, LocalFolderStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(options);

        _fileSystem = fileSystem;
        _folderPath = options.FolderPath;
    }

    /// <summary>
    /// Reads the mock response content associated with the given identifier from the local file system.
    /// </summary>
    /// <param name="identifier">The filename (e.g. "response.json") corresponding to the mock response.</param>
    /// <returns>The deserialized content and the provider name.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file does not exist at the computed path.
    /// </exception>
    public async Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier)
    {
        var filePath = Path.Combine(_folderPath, identifier);
        if (!_fileSystem.Exists(filePath))
        {
            throw new FileNotFoundException($"Unable to locate [{filePath}]");
        }

        return (await _fileSystem.ReadAllTextAsync(filePath), Name);
    }
}