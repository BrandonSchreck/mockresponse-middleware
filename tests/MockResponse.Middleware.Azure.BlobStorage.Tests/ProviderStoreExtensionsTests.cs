using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MockResponse.Middleware.Azure.BlobStorage.Tests;

[TestClass]
public class ProviderStoreExtensionsTests
{
    private const string ConnectionStringKey = "MockOptions:BlobStorageOptions:ConnectionString";
    private const string ContainerNameKey = "MockOptions:BlobStorageOptions:ContainerName";

    [TestMethod]
    public void AddAzureBlobStorage_Throws_InvalidOperationException_When_ConfigSection_Is_Missing()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddApiMocking(emptyConfig);

        // Act/Assert
        Assert.ThrowsException<InvalidOperationException>(() => services.AddAzureBlobStorage());
    }

    [TestMethod]
    public void AddAzureBlobStorage_With_Custom_Path_Throws_InvalidOperationException_When_Section_Is_Missing()
    {
        // Arrange
        const string customPath = "MyMissing:Section";

        var services = new ServiceCollection().AddApiMocking(
            new ConfigurationBuilder().Build()
        );

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => services.AddAzureBlobStorage(customPath));
    }

    [TestMethod]
    public void AddAzureBlobStorage_Should_Throw_OptionsValidationException_If_Missing_Option_Field()
    {
        // Arrange
        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { ConnectionStringKey, "connectionString" }
        });
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);

        // Act
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>();

        // Assert
        Assert.ThrowsException<OptionsValidationException>(() => options.CurrentValue);
    }

    [DataTestMethod]
    [DataRow(null, "containerName")]
    [DataRow("", "containerName")]
    [DataRow("connectionString", null)]
    [DataRow("connectionString", "")]
    public void AddAzureBlobStorage_Should_Throw_OptionsValidationException_If_Options_Has_Invalid_Data(string connectionString, string containerName)
    {
        // Arrange
        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { ConnectionStringKey, connectionString },
            { ContainerNameKey, containerName }
        });
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);

        // Act
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>();

        // Assert
        Assert.ThrowsException<OptionsValidationException>(() => options.CurrentValue);
    }

    [TestMethod]
    public void AddAzureBlobStorage_Registers_Options_And_Provider()
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
        var hasFactory = builder.Services.Any(s =>
            s.ServiceType == typeof(IBlobContainerClientFactory) &&
            s.ImplementationType == typeof(BlobContainerClientFactory)
        );
        var hasProvider = builder.Services.Any(s =>
            s.ServiceType == typeof(IMockResponseProvider) && s.ImplementationFactory != null
        );
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>();

        // Assert
        Assert.IsTrue(hasFactory);
        Assert.IsTrue(hasProvider);
        Assert.AreEqual(options.CurrentValue.ConnectionString, connectionString);
        Assert.AreEqual(options.CurrentValue.ContainerName, containerName);
    }

    [TestMethod]
    public void AddAzureBlobStorage_Should_Bind_With_ConfigurationSectionPath()
    {
        // Arrange
        const string customPath = "CustomRoot:Settings";
        const string connectionString = "Some:Connection-String";
        const string containerName = "container-name";

        var builder = SetupBuilder(
            initialData: new Dictionary<string, string?>
            {
                { $"{customPath}:ConnectionString", connectionString },
                { $"{customPath}:ContainerName", containerName },
            },
            configurationSectionPath: customPath
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>();

        // Assert
        Assert.AreEqual(connectionString, options.CurrentValue.ConnectionString);
        Assert.AreEqual(containerName, options.CurrentValue.ContainerName);
    }

    [TestMethod]
    public void Adding_Multiple_Providers_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { ConnectionStringKey, "connectionString" },
            { ContainerNameKey, "containerName" }
        });

        // Act/Assert
        Assert.ThrowsException<InvalidOperationException>(() => builder.AddAzureBlobStorage());
    }

    [TestMethod]
    public void Passed_ConfigOptions_Should_Override_Configuration()
    {
        // Arrange
        const string containerNameOverride = "another-container";
        var builder = SetupBuilder(
            initialData: new Dictionary<string, string?>
            {
                { ConnectionStringKey, "connectionString" },
                { ContainerNameKey, "containerKey" }
            },
            configOptions: opts => opts.ContainerName = containerNameOverride
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.AreEqual(
            serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>().CurrentValue.ContainerName,
            containerNameOverride
        );
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