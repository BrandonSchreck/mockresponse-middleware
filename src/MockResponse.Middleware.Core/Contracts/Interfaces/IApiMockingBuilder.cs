using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Provides access to core services and configuration used to register mock response providers.
/// </summary>
public interface IApiMockingBuilder
{
    /// <summary>
    /// Gets the current <see cref="IServiceCollection"/> used to register services.
    /// </summary>
    IServiceCollection Services { get; }
    
    /// <summary>
    /// Gets the application <see cref="IConfiguration"/> used for binding options and settings.
    /// </summary>
    IConfiguration Configuration { get; }
}