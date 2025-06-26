using Microsoft.Extensions.Configuration;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;

namespace MockResponse.Middleware.Core.Internal.Factories;

/// <summary>
/// Encapsulates the contextual information required to instantiate a mock response provider at runtime.
/// </summary>
/// <typeparam name="TOptions">
/// The strongly-typed options class bound from configuration.
/// </typeparam>
/// <typeparam name="TProvider">
/// The provider implementation to be created at runtime.
/// </typeparam>
internal class ProviderFactoryContext<TOptions, TProvider> : IProviderFactoryContext<TOptions, TProvider>
    where TOptions : class, IProviderOptions, new()
    where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
{
    /// <summary>
    /// Application's configuration used to retrieve and bind options values.
    /// </summary>
    public IConfiguration Configuration { get; init; } = default!;
    /// <summary>
    /// Optional delegate to override or customize the bound <typeparamref name="TOptions"/> values.
    /// </summary>
    public Action<TOptions>? ConfigureOptions { get; init; }
    /// <summary>
    /// Full configuration path used to bind <typeparamref name="TOptions"/> from <see cref="IConfigurationSection"/>.
    /// </summary>
    public string ConfigurationSectionPath { get; init; } = default!;
    /// <summary>
    /// The delegate used to instantiate a <typeparamref name="TProvider"/> using the resolved options and service.
    /// </summary>
    public Func<TOptions, IServiceProvider, TProvider> Initializer { get; init; } = default!;
    /// <summary>
    /// Logical name of the provider, used for diagnostics and validation.
    /// </summary>
    public string ProviderName { get; init; } = default!;
    /// <summary>
    /// ServiceProvider used to resolve dependencies during provider construction.
    /// </summary>
    public IServiceProvider ServiceProvider { get; init; } = default!;
}