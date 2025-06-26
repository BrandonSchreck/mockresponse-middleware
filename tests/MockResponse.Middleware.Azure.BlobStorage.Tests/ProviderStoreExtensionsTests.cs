using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MockResponse.Middleware.Azure.BlobStorage.Tests;

[TestClass]
public class ProviderStoreExtensionsTests
{
    private const string ConnectionStringKey = "MockOptions:BlobStorageOptions:ConnectionString";
    private const string ContainerNameKey = "MockOptions:BlobStorageOptions:ContainerName";

    [TestMethod]
    public void AddAzureBlobStorage_Registers_BlobContainerClientFactory_And_ProviderFactories()
    {
        // Arrange
        const string connectionString = "connectionString";
        const string containerName = "containerName";

        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { ConnectionStringKey, connectionString },
            { ContainerNameKey, containerName }
        });

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IBlobContainerClientFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<BlobStorageOptions, BlobStorageMockResponseProvider>>());
    }

    [TestMethod]
    public void AddAzureBlobStorage_With_Custom_Section_Registers_BlobContainerClientFactory_And_ProviderFactories()
    {
        // Arrange
        const string connectionString = "connectionString";
        const string containerName = "containerName";

        var builder = SetupBuilder(
            new Dictionary<string, string?>
            {
                { ConnectionStringKey, connectionString },
                { ContainerNameKey, containerName }
            },
            configOptions: opts =>
            {
                opts.ConnectionString = "anotherConnectionString";
                opts.ContainerName = "anotherContainerName";
            }
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IBlobContainerClientFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<BlobStorageOptions, BlobStorageMockResponseProvider>>());
    }

    [TestMethod]
    public void Adding_Multiple_Providers_Should_Throw_InvalidOperationException()
    {
        // Arrange
        const string connectionString = "connectionString";
        const string containerName = "containerName";

        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { ConnectionStringKey, connectionString },
            { ContainerNameKey, containerName }
        });

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => builder.AddAzureBlobStorage());
    }

    [TestMethod]
    public void AddAzureBlobStorage_With_Missing_ConfigurationSection_Still_Registers_Successfully()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddApiMocking(config);

        var builder = services.AddAzureBlobStorage();

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<IBlobContainerClientFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory>());
        Assert.IsNotNull(serviceProvider.GetService<IMockProviderFactory<BlobStorageOptions, BlobStorageMockResponseProvider>>());
    }

    private static IApiMockingBuilder SetupBuilder(Dictionary<string, string?> initialData, string? configurationSectionPath = null, Action<BlobStorageOptions>? configOptions = null)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();

        var services = new ServiceCollection().AddApiMocking(config);

        return configurationSectionPath is null
            ? services.AddAzureBlobStorage(configOptions)
            : services.AddAzureBlobStorage(configurationSectionPath, configOptions);
    }    
}