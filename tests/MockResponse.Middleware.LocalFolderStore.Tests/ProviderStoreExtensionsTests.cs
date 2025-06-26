using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.LocalFolderStore.Tests;

[TestClass]
public class ProviderStoreExtensionsTests
{
    private const string FolderPathKey = "MockOptions:LocalFolderStoreOptions:FolderPath";

    [TestMethod]
    public void AddLocalFolderStore_Registers_FileSystem_And_ProviderFactories()
    {
        // Arrange
        const string expectedFolderName = "SomeFolder";

        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { FolderPathKey, expectedFolderName }
        });

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IFileSystem>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<LocalFolderStoreOptions,LocalFolderStoreMockResponseProvider>>());
    }

    [TestMethod]
    public void AddLocalFolderStore_With_Custom_Section_Registers_FileSystem_And_ProviderFactories()
    {
        // Arrange
        const string expectedFolderName = "SomeFolder";

        var builder = SetupBuilder(
            new Dictionary<string, string?>
            {
                { FolderPathKey, expectedFolderName }
            },
            configOptions: opts =>
            {
                opts.FolderPath = "AnotherFolder";
            }
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IFileSystem>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<LocalFolderStoreOptions,LocalFolderStoreMockResponseProvider>>());
    }

    [TestMethod]
    public void Adding_Multiple_Providers_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { FolderPathKey, "SomeFolder" }
        });

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => builder.AddLocalFolderStore());
    }

    [TestMethod]
    public void AddLocalFolderStore_With_Missing_ConfigurationSection_Still_Registers_Successfully()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddApiMocking(config);

        var builder = services.AddLocalFolderStore();

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IFileSystem>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<LocalFolderStoreOptions,LocalFolderStoreMockResponseProvider>>());
    }

    private static IApiMockingBuilder SetupBuilder(Dictionary<string, string?> initialData, string? configurationSectionPath = null, Action<LocalFolderStoreOptions>? configOptions = null)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();

        var services = new ServiceCollection().AddApiMocking(config);

        return configurationSectionPath is null
            ? services.AddLocalFolderStore(configOptions)
            : services.AddLocalFolderStore(configurationSectionPath, configOptions);
    }
}