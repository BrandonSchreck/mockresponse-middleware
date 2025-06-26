using Microsoft.Extensions.Configuration;
using MockResponse.Middleware.Core.Options;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Represents a non-generic context used during mock provider factory creation, containing
/// shared configuration metadata and dependency resolution scope.
/// </summary>
internal interface IProviderFactoryContext
{
    /// <summary>
    /// Application's configuration used to retrieve and bind options values.
    /// </summary>
    IConfiguration Configuration { get; }
    /// <summary>
    /// Full configuration path used to bind <typeparamref name="TOptions"/> from <see cref="IConfigurationSection"/>.
    /// </summary>
    string ConfigurationSectionPath { get; }
    /// <summary>
    /// Logical name of the provider, used for diagnostics and validation.
    /// </summary>    
    string ProviderName { get; }
    /// <summary>
    /// ServiceProvider used to resolve dependencies during provider construction.
    /// </summary>    
    IServiceProvider ServiceProvider { get; }
}

/// <summary>
/// Represents a strongly-typed context used to construct a mock response provider, including
/// configuration binding, optional customization, and instance creation logic.
/// </summary>
/// <typeparam name="TOptions">
/// The strongly-typed options class bound from configuration.
/// </typeparam>
/// <typeparam name="TProvider">
/// The provider implementation to be created at runtime.
/// </typeparam>
internal interface IProviderFactoryContext<TOptions, TProvider> : IProviderFactoryContext
    where TOptions : class, IProviderOptions, new()
    where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
{
    /// <summary>
    /// Optional delegate to override or customize the bound <typeparamref name="TOptions"/> values.
    /// </summary>
    Action<TOptions>? ConfigureOptions { get; }
    /// <summary>
    /// The delegate used to instantiate a <typeparamref name="TProvider"/> using the resolved options and service.
    /// </summary>    
    Func<TOptions, IServiceProvider, TProvider> Initializer { get; }
}