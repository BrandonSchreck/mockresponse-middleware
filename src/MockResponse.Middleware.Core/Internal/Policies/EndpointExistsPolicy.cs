using System.Runtime.CompilerServices;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;

[assembly:InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Policies;

/// <summary>
/// A mocking policy that bypasses mocking if no endpoint is associated with the current request.
/// </summary>
/// <remarks>
/// If no endpoint is found for the current <see cref="HttpContext"/>, the policy bypasses the mocking
/// middleware so ASP.NET Core's default behavior is preserved. In this case, the request continue
/// through the pipeline and typically result in a 404 Not Found response, as handled by the framework's
/// built-in routing system.
/// </remarks>
internal sealed class EndpointExistsPolicy : IMockingPolicy
{
    /// <summary>
    /// Determines whether the current request should bypass mocking due to the absence of a matched endpoint.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="reason">When bypassing, contains a string describing the reason (e.g. "No endpoint found"); otherwise, <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> if no endpoint is found for the current request, indicating the mocking should be bypassed; otherwise, <c>false</c>.
    /// </returns>
    public bool ShouldBypass(HttpContext context, out string reason)
    {
        var nullEndpoint = context.GetEndpoint() is null;
        reason = nullEndpoint ? "No endpoint found" : null!;
        return nullEndpoint;
    }
}