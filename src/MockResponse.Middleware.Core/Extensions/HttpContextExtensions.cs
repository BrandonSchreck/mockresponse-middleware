using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests.Extensions")]
namespace MockResponse.Middleware.Core.Extensions;

/// <summary>
/// Extension methods for writing mock responses to an <see cref="HttpContext"/>
/// </summary>
internal static class HttpContextExtensions
{
    /// <summary>
    /// Writes a plain response to the HTTP <see cref="HttpResponse.Body"/> with the specified status code and content type.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to write the response to.</param>
    /// <param name="response">The response content to write.</param>
    /// <param name="statusCode">The HTTP status code to set on the response.</param>
    /// <param name="contentType">The content type of the response. Defaults to <c>text/plain</c>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal static async Task WriteResponseAsync(this HttpContext context, string response, int statusCode, string contentType = "text/plain")
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        await context.Response.WriteAsync(response);
    }

    /// <summary>
    /// Writes a JSON mock response to the <see cref="HttpResponse.Body"/> and appends provider metadata headers.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to write the response to.</param>
    /// <param name="identifier">The identifier of the mock response (e.g. filename).</param>
    /// <param name="providerType">The name of the provider that served the mock response.</param>
    /// <param name="response">The JSON content to write as the response body.</param>
    /// <param name="statusCode">The HTTP status code to set on the response. Defaults to <c>200  OK</c>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal static async Task WriteResponseAsync(this HttpContext context, string identifier, string providerType, string response, int statusCode = StatusCodes.Status200OK)
    {
        context.Response.Headers.Append("X-Mock-Identifier", identifier);
        context.Response.Headers.Append("X-Mock-Provider", providerType);
        await context.WriteResponseAsync(response, statusCode, "application/json");
    }
}