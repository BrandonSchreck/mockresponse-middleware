namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Represents a unique cache key for identifying an Azure <see cref="BlobContainerClient"/> based 
/// on its connection string and container name.
/// </summary>
/// <param name="connectionString">The Azure Blob Storage connection string.</param>
/// <param name="containerName">The Azure Blob Storage container name.</param>
internal readonly struct BlobClientCacheKey(string connectionString, string containerName) : IEquatable<BlobClientCacheKey>
{
    /// <summary>
    /// Gets the Azure Blob Storage connection string.
    /// </summary>
    public string ConnectionString { get; } = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    /// <summary>
    /// Gets the name of the Blob Storage container.
    /// </summary>
    public string ContainerName { get; } = containerName ?? throw new ArgumentNullException(nameof(containerName));

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is BlobClientCacheKey other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(BlobClientCacheKey other) =>
        ConnectionString == other.ConnectionString && ContainerName == other.ContainerName;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(ConnectionString, ContainerName);

    /// <summary>
    /// Returns a string representation of the cache key.
    /// </summary>
    public override string ToString() =>
        $"{ConnectionString}::{ContainerName}";

    /// <summary>
    /// Determines whether two <see cref="BlobClientCacheKey"/> instances are equal.
    /// </summary>    
    public static bool operator ==(BlobClientCacheKey left, BlobClientCacheKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="BlobClientCacheKey"/> instances are not equal.
    /// </summary>
    public static bool operator !=(BlobClientCacheKey left, BlobClientCacheKey right)
    {
        return !(left == right);
    }
}