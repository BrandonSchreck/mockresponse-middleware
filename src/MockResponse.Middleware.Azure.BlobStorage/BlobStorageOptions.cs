using MockResponse.Middleware.Core.Options;
using System.ComponentModel.DataAnnotations;

namespace MockResponse.Middleware.Azure.BlobStorage;

/// <summary>
/// Azure Blob Storage configuration details.
/// </summary>
public record BlobStorageOptions : IProviderOptions
{
    /// <summary>
    /// The configuration section name used to bind options.
    /// </summary>
    public static string SectionName => nameof(BlobStorageOptions);

    /// <summary>
    /// The Azure Blob Storage connection string used to connect to the storage account.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// The name of the Blob Storage container that stores mock responses.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ContainerName { get; set; } = default!;
}