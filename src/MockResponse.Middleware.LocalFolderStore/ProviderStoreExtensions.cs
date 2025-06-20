using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.LocalFolderStore;

/// <summary>
/// Extension methods for registering the local folder-based mock response provider.
/// </summary>
public static class ProviderStoreExtensions
{
    /// <summary>
    /// Registers a mock response provider that reads mock data from a local file system directory
    /// using the default configuration section: "MockOptions:LocalFolderStoreOptions".
    /// </summary>
    /// <param name="builder">The <see cref="IApiMockingBuilder"/> used for service configuration.</param>
    /// <param name="configOptions">Optional delegate to configure <see cref="LocalFolderStoreOptions"/>.</param>
    /// <returns>Configured <see cref="IApiMockingBuilder"/>.</returns>
    public static IApiMockingBuilder AddLocalFolderStore(this IApiMockingBuilder builder, Action<LocalFolderStoreOptions>? configOptions = null)
    {
        builder.Services.AddSingleton<IFileSystem, FileSystem>();

        return builder.AddStore<LocalFolderStoreOptions, LocalFolderStoreMockResponseProvider>(
            LocalFolderStoreDefaults.Name,
            configOptions
        );
    }

    /// <summary>
    /// Registers a mock response provider that reads mock data from a local file system directory
    /// using a custom configuration section path (e.g. "MyCustomConfig:ProviderSettings")
    /// </summary>
    /// <param name="builder">The <see cref="IApiMockingBuilder"/> used for service configuration.</param>
    /// <param name="configurationSectionPath">The full configuration path to bind options from.</param>
    /// <param name="configOptions">Optional delegate to configure <see cref="LocalFolderStoreOptions"/>.</param>
    /// <returns>Configured <see cref="IApiMockingBuilder"/>.</returns>
    public static IApiMockingBuilder AddLocalFolderStore(this IApiMockingBuilder builder, string configurationSectionPath, Action<LocalFolderStoreOptions>? configOptions = null)
    {
        builder.Services.AddSingleton<IFileSystem, FileSystem>();

        return builder.AddStore<LocalFolderStoreOptions, LocalFolderStoreMockResponseProvider>(
            LocalFolderStoreDefaults.Name,
            configurationSectionPath,
            configOptions
        );
    }
}