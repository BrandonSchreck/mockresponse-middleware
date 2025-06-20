using Microsoft.AspNetCore.Http;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Resolves a mock reference for a given <see cref="HttpContext"/> based on the request metadata and headers.
/// </summary>
public interface IMockReferenceResolver
{
    /// <summary>
    /// Attempts to resolve a <see cref="MockReferenceResult"/> from the current HTTP request.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> for the request being processed.</param>
    /// <param name="selectionResult">Contains the <see cref="MockReferenceResult"/> representing either a resolved reference or the reason for failure.</param>
    /// <returns><c>true</c> if a valid <see cref="MockReference"/> was found and can be used to retrieve a mock response; otherwise, <c>false</c>.</returns>
    bool TryGetMockReferenceResult(HttpContext context, out MockReferenceResult selectionResult);
}