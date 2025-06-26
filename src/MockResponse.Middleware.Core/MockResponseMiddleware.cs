using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace MockResponse.Middleware.Core;

/// <summary>
/// Middleware that intercepts HTTP requests and returns mock responses based on configuration and policies.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="policies">A collection of policies used to determine whether a request should bypass mocking.</param>
/// <param name="resolver">Resolves mock reference metadata for a given <see cref="HttpContext"/>.</param>
/// <param name="provider">Provides the mock response content based on the resolved reference.</param>
/// <param name="logger">Logger used to emit diagnostic information.</param>
public class MockResponseMiddleware(RequestDelegate next, IEnumerable<IMockingPolicy> policies, IMockReferenceResolver resolver, IMockProviderFactory factory, ILogger<MockResponseMiddleware> logger)
{
    /// <summary>
    /// Processes the incoming HTTP request and serves a mock response if applicable.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> of the current request.</param>
    /// <returns>A task that represents the asynchronous  operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            string? reason = null;
            if (policies.Any(x => x.ShouldBypass(context, out reason)))
            {
                logger.LogDebug("{Reason}, skipping mocking", reason);
                await next(context);
                return;
            }

            if (!resolver.TryGetMockReferenceResult(context, out var result))
            {
                logger.LogWarning("{Error}", result.ErrorMessage);
                await context.WriteResponseAsync(result.ErrorMessage!, result.StatusCode);
                return;
            }

            IMockResponseProvider provider = factory.Create();

            var (response, providerName) = await provider.GetMockResponseAsync(result.Reference!.Identifier);
            logger.LogInformation("Serving mock {Key} via {Provider}", result.Reference.Key, providerName);
            await context.WriteResponseAsync(result.Reference.Identifier, providerName, response, result.StatusCode);
        }
        catch (FileNotFoundException ex)
        {
            const string errorMessage = "Mock file was not found.";
            logger.LogError(ex, "{ErrorMessage} {ExceptionMessage}", errorMessage, ex.Message);
            await context.WriteResponseAsync(errorMessage, StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            const string errorMessage = "An application configuration error occurred. See logs for more details.";
            logger.LogCritical(ex,"{ErrorMessage} {ExceptionMessage}", errorMessage, ex.Message);
            await context.WriteResponseAsync(errorMessage, StatusCodes.Status500InternalServerError);
        }
        catch (ValidationException ex)
        {
            const string errorMessage = "Application configuration is missing or invalid. See logs for more details.";
            logger.LogCritical(ex, "{ErrorMessage} {ExceptionMessage}", errorMessage, ex.Message);
            await context.WriteResponseAsync(errorMessage, StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            const string errorMessage = "An unhandled error occurred.";
            logger.LogError(ex, "{ErrorMessage} {ExceptionMessage}", errorMessage, ex.Message);
            await context.WriteResponseAsync($"{errorMessage} - {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}