using MockResponse.Middleware.Core.Options;

namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Non-generic factory abstraction used to create the configured mock response provider. This interface is consumed
/// by the middleware and other runtime components that do not need to know the specific provider or options type.
/// </summary>
public interface IMockProviderFactory
{
    /// <summary>
    /// Creates an instance of the mock response provider using the current configuration and registered services.
    /// </summary>
    /// <returns>Fully configured <see cref="IMockResponseProvider"/> instance.</returns>
    IMockResponseProvider Create();
}

/// <summary>
/// Defines a generic factory for creating a specific <see cref="IMockResponseProvider"/> using strongly typed
/// configuration options and a runtime initializer.
/// </summary>
/// <typeparam name="TOptions">
/// The strongly-typed options class bound from configuration.
/// </typeparam>
/// <typeparam name="TProvider">
/// The provider implementation to be created at runtime.
/// </typeparam>
public interface IMockProviderFactory<TOptions, TProvider> : IMockProviderFactory
    where TOptions : class, IProviderOptions, new()
    where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
{
}