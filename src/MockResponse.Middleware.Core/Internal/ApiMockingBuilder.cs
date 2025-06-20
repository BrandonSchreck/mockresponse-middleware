using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Core.Internal;

/// <summary>
/// Provides an implementation of <see cref="IApiMockingBuilder"/> used to configure and extend API mocking behavior.
/// This builder is returned from <c>AddApiMocking</c> and is typically used to register mock response providers.
/// </summary>
/// <param name="services">The application's dependency injection service collection.</param>
/// <param name="configuration">The application's  configuration instance.</param>
internal sealed class ApiMockingBuilder(IServiceCollection services, IConfiguration configuration) : IApiMockingBuilder
{
    /// <summary>
    /// Gets the application's  service collection where mocking-related services can be registered.
    /// </summary>
    public IServiceCollection Services { get; } = services;
    
    /// <summary>
    /// Gets the application's configuration used for binding provider-specific and global mocking options.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration;
}