using MockResponse.Middleware.Core.Contracts;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Resolvers;

/// <summary>
/// Resolves a mock reference from <see cref="HttpContext"/> information, including metadata and header-based inputs.
/// </summary>
/// <param name="metadataResolver">Resolves metadata associated with the endpoint and status code.</param>
/// <param name="mappingResolver">Resolves mock response mappings based on metadata and variants.</param>
internal sealed class MockReferenceResolver(IEndpointMetadataResolver metadataResolver, IResponseMappingResolver mappingResolver) : IMockReferenceResolver
{
    /// <summary>
    /// Attempts to resolve a mock reference based on the current <see cref="HttpRequest"/> context. 
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> from which headers and endpoint metadata are derived.</param>
    /// <param name="selectionResult">Contains the resolved <see cref="MockReferenceResult"/> indicating success or failure details.</param>
    /// <returns><c>true</c> if a valid <see cref="MockReference"/> was found and populated in <paramref name="selectionResult"/>; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The resolution is driven by the presence of two optional headers:
    /// <list type="bullet">
    ///     <item><description><c>X-Mock-Status</c> - used to resolve metadata for a specific status code</description></item>
    ///     <item><description><c>X-Mock-Variant</c> - optional variant identifier used to select a specific mock mapping.</description></item>
    /// </list>
    /// If the status code cannot be parsed, the method returns a 400 Bad Request result is returned.
    /// If no metadata exists for the provided status, a 501 Not Implemented result is returned.
    /// If metadata exists but no mock mapping can be resolved, a 404 Not Found result is returned.
    /// </remarks>
    public bool TryGetMockReferenceResult(HttpContext context, out MockReferenceResult selectionResult)
    {
        var endpointName = context.GetEndpoint()!.DisplayName;
        var hdr = context.Request.Headers;
        var variant = hdr["x-mock-variant"].FirstOrDefault();
        
        var mockStatus = hdr["x-mock-status"].FirstOrDefault();
        if (string.IsNullOrEmpty(mockStatus))
        {
            selectionResult = MockReferenceResult.NotFound(
                "Missing required 'X-Mock-Status' header.",
                StatusCodes.Status400BadRequest
            );
            return false;
        }

        if (!int.TryParse(mockStatus, out var statusCode))
        {
            selectionResult = MockReferenceResult.NotFound(
                $"'{mockStatus}' is not a valid StatusCode.",
                StatusCodes.Status400BadRequest
            );
            return false;
        }

        var metaData = metadataResolver.GetMetadata(context, statusCode);
        if (metaData is { Count: 0 })
        {
            selectionResult = MockReferenceResult.NotFound(
                $"No [{statusCode}] status code metadata was found for endpoint [{endpointName}]",
                StatusCodes.Status501NotImplemented
            );
            return false;
        }

        if (mappingResolver.TryGetMockReference(metaData, variant, out var reference))
        {
            selectionResult = MockReferenceResult.Found(
                reference!,
                statusCode
            );
            return true;
        }

        var errorMessage = $"No [{statusCode}] status code mapping was found for endpoint [{endpointName}]";
        if (!string.IsNullOrWhiteSpace(variant))
        {
            errorMessage = $"{errorMessage} and variant [{variant}]";
        }

        selectionResult = MockReferenceResult.NotFound(
            errorMessage,
            StatusCodes.Status404NotFound
        );
        return false;
    }
}