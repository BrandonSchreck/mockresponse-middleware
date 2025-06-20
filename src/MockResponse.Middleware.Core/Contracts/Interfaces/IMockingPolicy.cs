using Microsoft.AspNetCore.Http;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Defines a policy to determine if a mock response should be bypassed for a given HTTP request.
/// </summary>
public interface IMockingPolicy
{
    /// <summary>
    /// Evaluates the current <see cref="HttpContext"/> and determines whether mocking should be bypassed.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="reason">When <c>true</c>, contains why mocking was bypassed; otherwise, <c>false</c>.</param>
    /// <returns><c>true</c> if mocking should be bypassed based on the policy; otherwise, <c>false</c>.</returns>
    bool ShouldBypass(HttpContext context, out string reason);
}