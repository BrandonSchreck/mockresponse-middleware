using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Azure.BlobStorage.Tests")]
namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Factory interface for creating instances of <see cref="BlobContainerClient"/>
/// </summary>
public interface IBlobContainerClientFactory
{
    /// <summary>
    /// Create a new instance of <see cref="BlobContainerClient"/> with the provided 
    /// connection string and container name.
    /// </summary>
    /// <param name="connectionString">The Azure Blob Storage connection string.</param>
    /// <param name="containerName">The Azure Blob Storage container name.</param>
    /// <returns>An initialized <see cref="BlobContainerClient"/>.</returns>
    BlobContainerClient Create(string connectionString, string containerName);
}

/// <summary>
/// Default implementation of <see cref="IBlobContainerClientFactory"/> that caches
/// <see cref="BlobContainerClient"/> instances based on connection string and container
/// name.
/// </summary>
internal sealed class BlobContainerClientFactory : IBlobContainerClientFactory
{
    private readonly ConcurrentDictionary<BlobClientCacheKey, BlobContainerClient> _cache = new();

    /// <summary>
    /// Creates or returns a cached <see cref="BlobContainerClient"/> instance based on the 
    /// provided connection string and container name.
    /// If a new combination is provided, the cache is cleared before the new client is added.
    /// </summary>
    /// <param name="connectionString">The Azure Blob Storage connection string.</param>
    /// <param name="containerName">The Azure Blob Storage container name.</param>
    /// <returns>A <see cref="BlobContainerClient"/> instance.</returns>
    public BlobContainerClient Create(string connectionString, string containerName)
    {
        var key = new BlobClientCacheKey(connectionString, containerName);

        return _cache.GetOrAdd(key, k =>
        {
            _cache.Clear();
            return new BlobContainerClient(k.ConnectionString, k.ContainerName);
        });
    }
}