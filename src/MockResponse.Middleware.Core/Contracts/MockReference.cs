namespace MockResponse.Middleware.Core.Contracts;

/// <summary>
/// Represents a reference to a mock response, including the blob identifier and key used.
/// </summary>
/// <param name="Identifier">The identifier used to locate the mock response (e.g. blob name, file name, or other identifier)</param>
/// <param name="Key">The key associated with the mock, often derived from metadata and variant information.</param>
public record MockReference(string Identifier, string Key);