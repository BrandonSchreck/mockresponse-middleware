using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Internal.Factories;
using MockResponse.Middleware.Core.Options;
using NSubstitute;

namespace MockResponse.Middleware.Core.Tests.Internal.Factories;

public class CustomOptions : IProviderOptions
{
    public static string SectionName => "Custom";
    
    [Required]
    public string? RequiredField { get; set; }

    public string? OptionalField { get; set; }
}

public class CustomProvider : IMockResponseProvider, IMockResponseProviderDefinition
{
    public string Name => ProviderName;
    public static string ProviderName => "CustomProvider";

    public Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier) => Task.FromResult(("{}", ProviderName));
}

[TestClass]
public sealed class DeferredMockProviderFactoryTests
{
    [TestMethod]
    public void Create_Throws_InvalidOperationException_If_Config_Section_Is_Missing()
    {
        // Arrange
        var context = ContextBuilder("Missing:Path", new ConfigurationBuilder().Build());
        var factory = new DeferredMockProviderFactory<CustomOptions, CustomProvider>(context);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => factory.Create());
    }

    [TestMethod]
    public void Create_Throws_ValidationException_If_Options_Validation_Fails()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                { "My:Path:OptionalField", "value" }
            })
            .Build();

        var context = ContextBuilder("My:Path", config);
        var factory = new DeferredMockProviderFactory<CustomOptions, CustomProvider>(context);

        // Act/Assert
        Assert.Throws<ValidationException>(() => factory.Create());
    }

    [TestMethod]
    public void Create_Applies_ConfigureOptions_Override()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                { "Root:OptionalField", "value" }
            })
            .Build();

        var context = ContextBuilder("Root", config, o => o.RequiredField = "set programmatically");
        var factory = new DeferredMockProviderFactory<CustomOptions, CustomProvider>(context);

        // Act
        var provider = factory.Create();

        // Assert
        Assert.IsNotNull(provider);
        Assert.IsInstanceOfType(provider, typeof(CustomProvider));
    }

    private static ProviderFactoryContext<CustomOptions, CustomProvider> ContextBuilder(string configSectionPath, IConfiguration config, Action<CustomOptions>? options = null) => new()
    {
        Configuration = config,
        ConfigurationSectionPath = configSectionPath,
        ProviderName = CustomProvider.ProviderName,
        ConfigureOptions = options,
        Initializer = (opts, _) => new CustomProvider(),
        ServiceProvider = Substitute.For<IServiceProvider>()
    };
}
