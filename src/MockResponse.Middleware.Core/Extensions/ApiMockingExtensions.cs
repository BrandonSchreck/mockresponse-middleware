using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Internal;
using MockResponse.Middleware.Core.Internal.Policies;
using MockResponse.Middleware.Core.Internal.Resolvers;
using MockResponse.Middleware.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Core.Extensions;

/// <summary>
/// Provides extension methods for configuring and enabling API mocking within an ASP.NET Core application.
/// </summary>
public static class ApiMockingExtensions
{
    /// <summary>
    /// Registers the core services required to enable API mocking support. This includes configuration binding,
    /// mocking policies, and resolution services. 
    /// </summary>
    /// <param name="services">The service collection used to register dependencies.</param>
    /// <param name="config">Application's configuration, used to bind <see cref="MockOptions"/>.</param>
    /// <returns>An <see cref="IApiMockingBuilder"/> that can be used to register additional mocking components.</returns>
    public static IApiMockingBuilder AddApiMocking(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MockOptions>(config.GetSection(MockOptions.SectionName));

        services.AddSingleton<IMockingPolicy, UseMockPolicy>();
        services.AddSingleton<IMockingPolicy, ExcludePathPolicy>();
        services.AddSingleton<IMockingPolicy, EndpointExistsPolicy>();

        services.AddSingleton<IMockReferenceResolver, MockReferenceResolver>();
        services.AddSingleton<IEndpointMetadataResolver, EndpointMetadataResolver>();
        services.AddSingleton<IResponseMappingResolver, ResponseMappingResolver>();

        return new ApiMockingBuilder(services,  config);
    }

    /// <summary>
    /// Enables the middleware responsible for serving mock responses when API mocking is configured.
    /// This should be placed early in the middleware pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure the middleware pipeline.</param>
    public static void UseApiMocking(this IApplicationBuilder app)
    {
        app.UseMiddleware<MockResponseMiddleware>();
    }
}