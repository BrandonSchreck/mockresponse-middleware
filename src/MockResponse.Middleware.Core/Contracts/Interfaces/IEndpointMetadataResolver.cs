using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Resolves endpoint metadata related to expected response types for a given <see cref="HttpContext"/>.
/// </summary>
public interface IEndpointMetadataResolver
{
    /// <summary>
    /// Retrieves metadata describing the expected response types for the specified status code from the current endpoint.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> of the incoming request.</param>
    /// <param name="statusCode">The HTTP status code to filter metadata by (e.g. 200, 404, etc.).</param>
    /// <returns>
    /// A read-only list of <see cref="IProducesResponseTypeMetadata"/> matching the given status code.
    /// If no metadata is found or the endpoint is missing, an empty list or exception may be returned depending on implementation.
    /// </returns>
    IReadOnlyList<IProducesResponseTypeMetadata> GetMetadata(HttpContext context, int statusCode);
}