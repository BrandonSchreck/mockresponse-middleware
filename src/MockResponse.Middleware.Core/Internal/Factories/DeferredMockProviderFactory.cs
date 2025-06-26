using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Factories;

/// <summary>
/// Provides a deferred, runtime-safe factory for creating a mock response provider. This is rquired when configuration for the 
/// provider (e.g. BlobStorage or LocalFolder) may be absent at startup...like in a production environment to prevent 
/// initialization.
/// </summary>
/// <typeparam name="TOptions">
/// The strongly-typed options class bound from configuration.
/// </typeparam>
/// <typeparam name="TProvider">
/// The provider implementation to be created at runtime.
/// </typeparam>
/// <param name="context">
/// The factory context containing configuration access, service provider, initialization logic, and metadata.
/// </param>
internal class DeferredMockProviderFactory<TOptions, TProvider>(IProviderFactoryContext<TOptions, TProvider> context) : IMockProviderFactory<TOptions, TProvider>
    where TOptions : class, IProviderOptions, new()
    where TProvider : class, IMockResponseProvider, IMockResponseProviderDefinition
{
    /// <summary>
    /// Laziliy creates and returns a configured mock response provider based on the current runtime configuration.
    /// </summary>
    /// <returns>
    /// A fully initialized and validated <typeparamref name="TProvider"/> instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the required configuration section is missing or contains invalid data.
    /// </exception>
    public IMockResponseProvider Create()
    {
        var section = context.Configuration.GetSection(context.ConfigurationSectionPath);
        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"Missing the configuration section for the '{context.ProviderName}' Mock Provider"
            );
        }

        var options = InitializeOptions(section);
        return context.Initializer(options, context.ServiceProvider);
    }

    /// <summary>
    /// Binds the configuration section to a new <typeparamref name="TOptions"/> instance and
    /// applies validation.
    /// </summary>
    /// <param name="section">Configuration section to bind.</param>
    /// <returns>A validated instance of <typeparamref name="TOptions"/>.</returns>
    /// <exception cref="ValidationException">
    /// Thrown if required configuration values are missing or fail data annotation validation.
    /// </exception>
    private TOptions InitializeOptions(IConfigurationSection section)
    {
        var options = new TOptions();
        section.Bind(options);

        context.ConfigureOptions?.Invoke(options);
        Validator.ValidateObject(options, new ValidationContext(options), true);

        return options;
    }
}