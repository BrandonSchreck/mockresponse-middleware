using MockResponse.Middleware.Core.Contracts.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Azure.BlobStorage.Tests")]
namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Provides mock HTTP responses by retrieving pre-defined JSON payloads from an Azure Blob Storage container.
/// </summary>
internal sealed class BlobStorageMockResponseProvider : IMockResponseProvider, IMockResponseProviderDefinition, IDisposable
{
    /// <summary>
    /// Gets the provider name used for registration and diagnostics.
    /// </summary>
    public string Name => ProviderName;

    /// <summary>
    /// The constant identifier used to register this provider in the DI container.
    /// </summary>
    public static string ProviderName => BlobStorageDefaults.Name;

    private readonly IBlobContainerClientFactory _factory;
    private readonly IDisposable? _onChangeListener;
    private readonly IOptionsMonitor<BlobStorageOptions> _options;

    private Lazy<BlobContainerClient> _lazyClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageMockResponseProvider"/> class.
    /// Sets up the container client and watches for configuration changes.
    /// </summary>
    /// <param name="factory">Factory used to create the <see cref="BlobContainerClient"/>.</param>
    /// <param name="options">Options Monitor for binding and reacting to changes in the <see cref="BlobStorageOptions"/> configuration.</param>
    public BlobStorageMockResponseProvider(IBlobContainerClientFactory factory, IOptionsMonitor<BlobStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);

        _factory = factory;
        _options = options;
        _lazyClient = CreateLazyClient(options.CurrentValue);
        _onChangeListener = options.OnChange(opts =>
        {
            _lazyClient = CreateLazyClient(opts);
        });
    }

    /// <summary>
    /// Retrieves the mock response content from the configured blob container.
    /// </summary>
    /// <param name="identifier">The blob name (identifier) to retrieve.</param>
    /// <returns>The mock response content and provider name.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
    public async Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier)
    {
        var container = _lazyClient.Value;
        var blobClient = container.GetBlobClient(identifier);
        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Unable to locate [{container.Name}\\{identifier}]");
        }

        var result = await blobClient.DownloadContentAsync();
        return (result.Value.Content.ToString(), Name);
    }

    /// <summary>
    /// Disposes the change listener registered with the <see cref="IOptionsMonitor{TOptions}"/>
    /// </summary>
    public void Dispose() => _onChangeListener?.Dispose();

    /// <summary>
    /// Creates a lazily initialized <see cref="BlobContainerClient"/> for the given configuration. 
    /// </summary>
    /// <param name="opts">The current <see cref="BlobStorageOptions"/> used to configure the client.</param>
    /// <returns>
    /// A <see cref="Lazy{T}"/> instance that initializes a <see cref="BlobContainerClient"/> on first access.
    /// </returns>
    /// <remarks>
    /// This method performs basic validation of the connection string and container name. If the client cannot
    /// connect to the specified container, an <see cref="InvalidOperationException"/> is thrown during
    /// lazy initialization.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown if the <see cref="BlobStorageOptions.ConnectionString"/> or
    /// <see cref="BlobStorageOptions.ContainerName"/> is null or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the blob container is inaccessible at initialization time.
    /// </exception>
    private Lazy<BlobContainerClient> CreateLazyClient(BlobStorageOptions opts) => new(() =>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opts.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(opts.ContainerName);

        var newClient = _factory.Create(opts.ConnectionString, opts.ContainerName);

        try
        {
            newClient.Exists(); // synchronous health check
            return newClient;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to access Azure Blob Container '{newClient.Name}': {ex.Message}";
            throw new InvalidOperationException(errorMessage, ex);
        }
    });
}