using Microsoft.AspNetCore.Http.Metadata;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Resolves a mock response reference based on API metadata and an optional variant.
/// </summary>
public interface IResponseMappingResolver
{
    /// <summary>
    /// Attempts to resolve a mock reference from the given response metadata and optional variant.
    /// </summary>
    /// <param name="metadata">A collection of response metadata describing the expected responses of an endpoint.</param>
    /// <param name="variant">An optional variant name to differentiate multiple mock responses for the same response type. e.g. "Error", "Empty", "Expired", etc.</param>
    /// <param name="reference">When successful, contains the resolved <see cref="MockReference"/> with identifier and key information.</param>
    /// <returns><c>true</c> if a matching mock reference was found; otherwise, <c>false</c>.</returns>
    bool TryGetMockReference(IReadOnlyList<IProducesResponseTypeMetadata> metadata, string? variant, out MockReference? reference);
}