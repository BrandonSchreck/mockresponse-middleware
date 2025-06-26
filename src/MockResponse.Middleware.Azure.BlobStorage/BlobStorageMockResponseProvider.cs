using MockResponse.Middleware.Core.Contracts.Interfaces;
using Azure.Storage.Blobs;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Azure.BlobStorage.Tests")]
namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Provides mock HTTP responses by retrieving pre-defined JSON payloads from an Azure Blob Storage container.
/// </summary>
internal sealed class BlobStorageMockResponseProvider : IMockResponseProvider, IMockResponseProviderDefinition
{
    /// <summary>
    /// Gets the provider name used for registration and diagnostics.
    /// </summary>
    public string Name => ProviderName;

    /// <summary>
    /// The constant identifier used to register this provider in the DI container.
    /// </summary>
    public static string ProviderName => BlobStorageDefaults.Name;

    private readonly BlobContainerClient _client;
    private readonly IBlobContainerClientFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageMockResponseProvider"/> class.
    /// </summary>
    /// <param name="factory">The factory used to create the <see cref="BlobContainerClient"/>.</param>
    /// <param name="options">The strongly-typed options containing Azure Blob Storage connection details.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the blob container is not accessible during initialization.</exception>
    public BlobStorageMockResponseProvider(IBlobContainerClientFactory factory, BlobStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);

        _factory = factory;
        _client = CreateClient(options);
    }

    /// <summary>
    /// Retrieves the mock response content from Azure Blob Storage.
    /// </summary>
    /// <param name="identifier">The blob name (i.e., identifier) to retrieve.</param>
    /// <returns>A tuple containing the raw mock response content and the provider name.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the blob with the given identifier does not exist.</exception>
    public async Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier)
    {
        var blobClient = _client.GetBlobClient(identifier);
        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Unable to locate [{_client.Name}\\{identifier}]");
        }

        var result = await blobClient.DownloadContentAsync();
        return (result.Value.Content.ToString(), Name);
    }

    /// <summary>
    /// Creates a new <see cref="BlobContainerClient"/> instance using the provided options,
    /// and verifies the container exists and is accessible.
    /// </summary>
    /// <param name="opts">The configuration options used to create the container client.</param>
    /// <returns>A valid and accessible <see cref="BlobContainerClient"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the container is inaccessible.</exception>
    private BlobContainerClient CreateClient(BlobStorageOptions opts)
    {
        var client = _factory.Create(opts.ConnectionString, opts.ContainerName);

        try
        {
            client.Exists();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unable to access the Azure Storage Container: {ex.Message}", ex
            );
        }

        return client;
    }
}