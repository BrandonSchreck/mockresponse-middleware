namespace MockResponse.Middleware.Core.Options;

/// <summary>
/// Configuration settings for controlling mock response behavior within the middleware.
/// </summary>
public class MockOptions
{
    /// <summary>
    /// The name of the configuration section used to bind this options class from configuration sources.
    /// </summary>
    public const string SectionName = nameof(MockOptions);

    /// <summary>
    /// Gets or sets a value indicating whether mock mode is enabled.
    /// When set to <c>true</c>, the middleware returns mock responses instead of  forwarding requests to the actual API endpoints.
    /// </summary>
    public bool UseMock { get; set; } = false;

    /// <summary>
    /// Gets or sets an array of request path segments that should be excluded from mock processing.
    /// These paths will bypass mocking and be forwarded to the next middleware or real endpoint.
    /// e.g. ["/swagger", "/redoc", "/api/health", "/metrics"].
    /// </summary>
    public string[] ExcludedRequestPaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a mapping between the fully qualified name of a response type and a mock identifier.
    /// The identifier is typically a filename used to locate the corresponding mock data.
    /// </summary>
    public Dictionary<string, string> ResponseMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}