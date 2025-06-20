using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using MockResponse.Middleware.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MockResponse.Middleware.Core.Tests.Extensions;

[TestClass]
// ReSharper disable ClassNeverInstantiated.Local
public class ProviderStoreExtensionsTests
{
    private class MismatchedOptions : IProviderOptions
    {
        public static string SectionName => nameof(MismatchedOptions);
    }

    private class MismatchedNameProvider : IMockResponseProvider, IMockResponseProviderDefinition
    {
        public string Name => ProviderName;
        public static string ProviderName => "SomeProviderName";

        public Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier) => Task.FromResult(("{}", ProviderName));
    }

    [TestMethod]
    public void AddStore_Should_Throw_InvalidOperationException_If_Provider_Name_Does_Not_Match_Expected()
    {
        // Arrange
        var services = new ServiceCollection();

        var builder = services.AddApiMocking(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>{
                    { "MockOptions:MismatchedOptions:SomeField", "SomeValue" }
                })
                .Build()
        );
        builder.AddStore<MismatchedOptions, MismatchedNameProvider>("SomeOtherProviderName");
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IMockResponseProvider>()
        );
    }

    private class CustomOptions : IProviderOptions
    {
        public static string SectionName => "ShouldBeIgnored";
        public string? FolderPath { get; set; }
    }

    private class CustomNamedProvider : IMockResponseProvider, IMockResponseProviderDefinition
    {
        public string Name => ProviderName;
        public static string ProviderName => "CustomNamedProvider";


        public Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier) => Task.FromResult(("{}", ProviderName));
    }

    [TestMethod]
    public void AddStore_Should_Bind_Options_From_Custom_Configuration_Section()
    {
        // Arrange;
        const string configurationPath = "CustomRoot:NestedSection";
        const string folderPath = "MyMocks";

        var services = new ServiceCollection();

        var builder = services.AddApiMocking(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"{configurationPath}:FolderPath", folderPath }
            })
            .Build()
        );
        builder.AddStore<CustomOptions, CustomNamedProvider>(CustomNamedProvider.ProviderName, configurationPath);
        
        // Act 
        var serviceProvider = builder.Services.BuildServiceProvider();
        var responseProvider = serviceProvider.GetRequiredService<IMockResponseProvider>();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<CustomOptions>>();

        // Assert
        Assert.IsInstanceOfType(responseProvider, typeof(CustomNamedProvider));
        Assert.AreEqual(options.CurrentValue.FolderPath, folderPath);
    }
}