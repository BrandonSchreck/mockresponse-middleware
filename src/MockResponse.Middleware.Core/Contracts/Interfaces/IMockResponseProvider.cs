namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Represents a mock response provider capable of retrieving predefined responses from a given source.
/// </summary>
public interface IMockResponseProvider
{
    /// <summary>
    /// Gets the logical name of the provider (e.g. "AzureBlobStorage", "LocalFolderStore", etc)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves the mock response content for the specified identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the mock response (e.g. a file name, blob name, database identifier, etc)</param>
    /// <returns></returns>
    Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier);
}