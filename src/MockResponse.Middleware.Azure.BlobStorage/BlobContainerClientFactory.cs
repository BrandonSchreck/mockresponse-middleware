using Azure.Storage.Blobs;

namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Factory interface for creating instances of <see cref="BlobContainerClient"/>
/// </summary>
public interface IBlobContainerClientFactory
{
    /// <summary>
    /// Create <see cref="BlobContainerClient"/> with the provided connection string and container name.
    /// </summary>
    /// <param name="connectionString">The Azure Blob Storage connection string.</param>
    /// <param name="containerName">The Azure Blob Storage container name.</param>
    /// <returns></returns>
    BlobContainerClient Create(string connectionString, string containerName);
}

/// <summary>
/// Default implementation of <see cref="IBlobContainerClientFactory"/>.
/// </summary>
public class BlobContainerClientFactory : IBlobContainerClientFactory
{
    /// <inheritdoc />
    public BlobContainerClient Create(string connectionString, string containerName)
    {
        return new BlobContainerClient(connectionString, containerName);
    }
}