using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Azure.BlobStorage;

public static class ProviderStoreExtensions
{
    /// <summary>
    /// Registers an Azure Blob Storage-backed mock provider to retrieve data from using
    /// the default configuration section: "MockOptions:BlobStorageOptions"
    /// </summary>
    /// <param name="builder"><see cref="IApiMockingBuilder"/> instance.</param>
    /// <param name="configOptions">Optional configuration override for <see cref="BlobStorageOptions"/></param>
    /// <returns>Modified <see cref="IApiMockingBuilder"/> instance.</returns>
    public static IApiMockingBuilder AddAzureBlobStorage(this IApiMockingBuilder builder, Action<BlobStorageOptions>? configOptions = null)
    {
        builder.Services.AddSingleton<IBlobContainerClientFactory, BlobContainerClientFactory>();

        return builder.AddStore<BlobStorageOptions, BlobStorageMockResponseProvider>(
            BlobStorageDefaults.Name,
            configOptions
        );
    }

    /// <summary>
    /// Registers an Azure Blob Storage-backed mock provider to retrieve data from using
    /// a custom configuration section path (e.g. "MyCustomConfig:ProviderSettings").
    /// </summary>
    /// <param name="builder"><see cref="IApiMockingBuilder"/> instance.</param>
    /// <param name="configurationSectionPath">The full configuration path to bind options from.</param>
    /// <param name="configOptions">Optional configuration override for <see cref="BlobStorageOptions"/></param>
    /// <returns>Modified <see cref="IApiMockingBuilder"/> instance.</returns>
    public static IApiMockingBuilder AddAzureBlobStorage(this IApiMockingBuilder builder, string configurationSectionPath, Action<BlobStorageOptions>? configOptions = null)
    {
        builder.Services.AddSingleton<IBlobContainerClientFactory, BlobContainerClientFactory>();

        return builder.AddStore<BlobStorageOptions, BlobStorageMockResponseProvider>(
            BlobStorageDefaults.Name,
            configurationSectionPath,
            configOptions
        );
    }
}