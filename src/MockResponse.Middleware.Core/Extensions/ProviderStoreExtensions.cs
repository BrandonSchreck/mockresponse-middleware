using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using MockResponse.Middleware.Core.Internal.Factories;

namespace MockResponse.Middleware.Core.Extensions;

/// <summary>
/// Extension methods for registering a deferred mock response provider with the API mocking system.
/// </summary>
public static class ProviderStoreExtensions
{

    /// <summary>
    /// Registers a deferred mock response provider using the default configuration section name defined by 
    /// <typeparam name="TOptions"/> "MockOptions.{TOptions.SectionName}".
    /// </summary>
    /// <typeparam name="TOptions">
    /// The strongly-typed options class bound from configuration.
    /// </typeparam>
    /// <typeparam name="TProvider">
    /// The provider implementation to be created at runtime.
    /// </typeparam>
    /// <param name="builder">The mocking builder used to configure services.</param>
    /// <param name="providerName">Logical name of the provider used for diagnostics and validation.</param>
    /// <param name="configureOptions">Optional delegate to override values programmatically.</param>
    /// <returns>The updated <see cref="IApiMockingBuilder"/> for chaining.</returns>
    /// <remarks>
    /// This method registers a factory that defers creation of the mock provider until it is first used.
    /// Any configuration-related exceptions will be thrown at runtime when the factory's <c>Create()</c> method is invoked.
    /// </remarks>
    public static IApiMockingBuilder AddStore<TOptions, TProvider>(this IApiMockingBuilder builder, string providerName, Action<TOptions>? configureOptions = null)
        where TOptions : class, IProviderOptions, new()
        where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
    {
        var defaultPath = $"{MockOptions.SectionName}:{TOptions.SectionName}";
        return builder.AddStore<TOptions, TProvider>(providerName, defaultPath, configureOptions);
    }

    /// <summary>
    /// Registers a deferred mock response provider and its associated configuration into the DI container.
    /// This enables runtime-safe resolution of mock response providers (e.g., Azure Blob, Local Folder, or custom).
    /// </summary>
    /// <typeparam name="TOptions">
    /// The strongly-typed options class bound from configuration.
    /// </typeparam>
    /// <typeparam name="TProvider">
    /// The provider implementation to be created at runtime.
    /// </typeparam>
    /// <param name="builder">The mocking builder used to configure services.</param>
    /// <param name="providerName">Logical name of the provider used for diagnostics and validation.</param>
    /// <param name="configurationSectionPath">Configuration section to bind options from (e.g., "MockOptions:BlobStorageOptions").</param>
    /// <param name="configureOptions">Optional delegate to override values programmatically.</param>
    /// <returns>The updated <see cref="IApiMockingBuilder"/> for chaining.</returns>
    /// <remarks>
    /// This method registers a factory that defers creation of the mock provider until it is first used.
    /// Any configuration-related exceptions will be thrown at runtime when the factory's <c>Create()</c> method is invoked.
    /// </remarks>
    public static IApiMockingBuilder AddStore<TOptions, TProvider>(this IApiMockingBuilder builder, string providerName, string configurationSectionPath, Action<TOptions>? configureOptions = null)
        where TOptions : class, IProviderOptions, new()
        where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
    {
        if (builder.Services.Any(service => service.ServiceType == typeof(IMockProviderFactory)))
        {
            throw new InvalidOperationException("Only one mock response provider can be registered.");
        }

        builder.Services.AddSingleton<IMockProviderFactory>(serviceProvider =>
        {
            var context = new ProviderFactoryContext<TOptions, TProvider>
            {
                Configuration = builder.Configuration,
                ConfigureOptions = configureOptions,
                ConfigurationSectionPath = configurationSectionPath,
                Initializer = (opts, sp) => ActivatorUtilities.CreateInstance<TProvider>(sp, opts),
                ProviderName = providerName,
                ServiceProvider = serviceProvider
            };

            return new DeferredMockProviderFactory<TOptions, TProvider>(context);
        });

        builder.Services.AddSingleton(sp => 
            (IMockProviderFactory<TOptions, TProvider>)sp.GetRequiredService<IMockProviderFactory>()
        );

        return builder;
    }
}