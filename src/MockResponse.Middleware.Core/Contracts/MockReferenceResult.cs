namespace MockResponse.Middleware.Core.Contracts;

/// <summary>
/// Represents the result of attempting to resolve a mock reference.
/// </summary>
/// <param name="WasFound">Indicates whether a matching mock reference was found.</param>
/// <param name="Reference">The resolved <see cref="MockReference"/>, or <c>null</c> if not found.</param>
/// <param name="StatusCode">The HTTP status code associated with the resolution attempt.</param>
/// <param name="ErrorMessage">An optional error message if the reference was not found.</param>
public record MockReferenceResult(bool WasFound, MockReference? Reference, int StatusCode, string? ErrorMessage)
{
    /// <summary>
    /// Creates a successful <see cref="MockReferenceResult"/> with the provided reference and status code.
    /// </summary>
    /// <param name="reference">The resolved <see cref="MockReference"/>.</param>
    /// <param name="statusCode">The HTTP status code to return (derived from the X-Mock-Status required header).</param>
    /// <returns>A <see cref="MockReferenceResult"/> representing a successful lookup.</returns>
    public static MockReferenceResult Found(MockReference reference, int statusCode) => new(true, reference, statusCode, null);

    /// <summary>
    /// Creates a failed <see cref="MockReferenceResult"/> with an error message and status code.
    /// </summary>
    /// <param name="error">The error message explaining why the reference could not be resolved.</param>
    /// <param name="statusCode">The HTTP status code to return (typically a 400/404).</param>
    /// <returns>A <see cref="MockReferenceResult"/> representing a failed lookup.</returns>
    public static MockReferenceResult NotFound(string error, int statusCode) => new(false, null, statusCode, error);
}