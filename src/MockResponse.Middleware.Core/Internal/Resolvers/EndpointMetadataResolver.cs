using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Resolvers;

/// <summary>
/// Resolves response metadata for a given <see cref="HttpContext"/> and status code.
/// </summary>
internal sealed class EndpointMetadataResolver : IEndpointMetadataResolver
{
    /// <summary>
    /// Retrieves metadata for the specified status code from the current endpoint.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> containing the endpoint.</param>
    /// <param name="statusCode">The HTTP status code used to filter metadata.</param>
    /// <returns>
    /// A read-only list of <see cref="IProducesResponseTypeMetadata"/> entries that match the status code,
    /// excluding those whose type is <see cref="IResult"/>.
    /// </returns>
    public IReadOnlyList<IProducesResponseTypeMetadata> GetMetadata(HttpContext context, int statusCode)
    {
        return context.GetEndpoint()?.Metadata.OfType<IProducesResponseTypeMetadata>()
            .Where(meta => meta.StatusCode == statusCode && meta.Type != typeof(IResult))
            .ToList() ?? new List<IProducesResponseTypeMetadata>();
    }
}