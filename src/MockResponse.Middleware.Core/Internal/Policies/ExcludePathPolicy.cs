using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Policies;

/// <summary>
/// A mocking policy that bypasses mock responses for specific request paths
/// configured in <see cref="MockOptions.ExcludedRequestPaths"/>.
/// </summary>
internal sealed class ExcludePathPolicy : IMockingPolicy
{
    private string[] _excludedRequestPaths = default!;
    private readonly IDisposable? _onChangeListener;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcludePathPolicy"/> class and
    /// registers for changes in <see cref="MockOptions"/>
    /// </summary>
    /// <param name="options">The monitor for <see cref="MockOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
    public ExcludePathPolicy(IOptionsMonitor<MockOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _onChangeListener = options.OnChange(OptionsChangeHandler);
        OptionsChangeHandler(options.CurrentValue);
    }

    /// <summary>
    /// Determines whether the current request should bypass mocking based on the request path.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="reason">If bypassing, contains a description of the reason; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the request path is excluded from mocking; otherwise, <c>false</c>.</returns>
    public bool ShouldBypass(HttpContext context, out string reason)
    {
        var requestPath = context.Request.Path;
        if (IsPathExcluded(_excludedRequestPaths, requestPath))
        {
            reason = $"'{requestPath}' path is excluded";
            return true;
        }

        reason = null!;
        return false;
    }

    /// <summary>
    /// Disposes of the options change listener to prevent memory leaks.
    /// </summary>
    public void Dispose() => _onChangeListener?.Dispose();

    /// <summary>
    /// Determines whether a request path matches any of the configured excluded paths.
    /// Matching is done using a case-insensitive prefix comparison.
    /// </summary>
    /// <param name="excludedRequestPaths">The list of excluded path segments.</param>
    /// <param name="requestPath">The incoming request path.</param>
    /// <returns><c>true</c> if the path should be excluded from mocking; otherwise, <c>false</c>.</returns>
    private static bool IsPathExcluded(IEnumerable<string> excludedRequestPaths, PathString requestPath)
    {
        var normalizedRequestPath = new PathString(Normalize(requestPath));

        return excludedRequestPaths.Any(
            path => normalizedRequestPath.StartsWithSegments(Normalize(path), StringComparison.OrdinalIgnoreCase)
        );

        string Normalize(string path) => path.TrimEnd('/');
    }

    /// <summary>
    /// Updates the list of excluded request paths when options change.
    /// </summary>
    /// <param name="options">The updated <see cref="MockOptions"/> instance.</param>
    private void OptionsChangeHandler(MockOptions options)
    {
        // ExcludedRequestPaths defaults to an empty array...no need to throw error
        // if empty
        _excludedRequestPaths = options.ExcludedRequestPaths;
    }
}