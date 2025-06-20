using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Core.Extensions;

/// <summary>
/// Extension methods for registering a custom mock response provider with the API mocking system.
/// </summary>
public static class ProviderStoreExtensions
{

    /// <summary>
    /// Registers a mock response provider using the default configuration section name defined by <typeparam name="TOptions"></typeparam>: "MockOptions.{TOptions.SectionName}".
    /// </summary>
    /// <typeparam name="TOptions">The options class type that implements <see cref="IProviderOptions"/>.</typeparam>
    /// <typeparam name="TProvider">The provider class that implements <see cref="IMockResponseProvider"/> and <see cref="IMockResponseProviderDefinition"/>.</typeparam>
    /// <param name="builder">The API mocking builder to register the provider with.</param>
    /// <param name="providerName">The logical name used to identify the provider implementation.</param>
    /// <param name="configureOptions">An optional delegate to further configure/override the provider's options.</param>
    /// <returns>The same <see cref="IApiMockingBuilder"/> instance for chaining.</returns>
    public static IApiMockingBuilder AddStore<TOptions, TProvider>(this IApiMockingBuilder builder, string providerName, Action<TOptions>? configureOptions = null)
        where TOptions : class, IProviderOptions
        where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
    {
        var defaultPath = $"{MockOptions.SectionName}:{TOptions.SectionName}";
        return builder.AddStore<TOptions, TProvider>(providerName, defaultPath, configureOptions);
    }

    /// <summary>
    /// Registers a mock response provider using a custom configuration section path.
    /// </summary>
    /// <typeparam name="TOptions">The options class type that implements <see cref="IProviderOptions"/>.</typeparam>
    /// <typeparam name="TProvider">The provider class that implements <see cref="IMockResponseProvider"/> and <see cref="IMockResponseProviderDefinition"/>.</typeparam>
    /// <param name="builder">The API mocking builder to register the provider with.</param>
    /// <param name="providerName">The logical name used to identify the provider implementation.</param>
    /// <param name="configurationSectionPath">The full configuration path to bind options from (e.g. "MyCustomConfig:ProviderSettings")</param>
    /// <param name="configureOptions">An optional delegate to further configure/override the provider's options.</param>
    /// <returns>The same <see cref="IApiMockingBuilder"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if:
    /// <list type="bullet">
    /// <item>Another provider has already been registered</item>
    /// <item>The specified configuration section does not exist</item>
    /// <item>The resolved provider's <c>Name</c> does not match the expected <param name="providerName"/>.</item>
    /// </list>
    /// </exception>
    public static IApiMockingBuilder AddStore<TOptions, TProvider>(this IApiMockingBuilder builder, string providerName, string configurationSectionPath, Action<TOptions>? configureOptions = null)
        where TOptions : class, IProviderOptions
        where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
    {
        if (builder.Services.Any(service => service.ServiceType == typeof(IMockResponseProvider)))
        {
            throw new InvalidOperationException("Only one mock response provider can be registered.");
        }

        var section = builder.Configuration.GetSection(configurationSectionPath);
        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"Missing configuration section '{configurationSectionPath}' for provider '{providerName}'"
            );
        }

        var options = builder.Services.AddOptions<TOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart();
        if (configureOptions != null)
        {
            options.Configure(configureOptions);
        }

        builder.Services.AddSingleton<TProvider>();
        builder.Services.AddSingleton<IMockResponseProvider>(sp =>
        {
            var provider = sp.GetRequiredService<TProvider>();
            if (provider.Name != providerName)
            {
                throw new InvalidOperationException(
                    $"Provider {typeof(TProvider).Name} has Name='{provider.Name}' but was registered as '{providerName}'"
                );
            }

            return provider;
        });

        return builder;
    }
}