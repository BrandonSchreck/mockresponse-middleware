using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MockResponse.Middleware.LocalFolderStore.Tests;

[TestClass]
public class ProviderStoreExtensionsTests
{
    private const string FolderPathKey = "MockOptions:LocalFolderStoreOptions:FolderPath";

    [TestMethod]
    public void AddLocalFolderStore_Throws_InvalidOperationException_When_ConfigSection_Is_Missing()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddApiMocking(emptyConfig);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => services.AddLocalFolderStore());
    }

    [TestMethod]
    public void AddLocalFolderStore_With_Custom_Path_Throws_InvalidOperationException_When_Section_Is_Missing()
    {
        // Arrange
        const string customPath = "MyMissing:Section";

        var services = new ServiceCollection().AddApiMocking(
            new ConfigurationBuilder().Build()
        );

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => services.AddLocalFolderStore(customPath));
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void AddLocalFolderStore_Throws_OptionsValidationException_When_Options_Has_Invalid_FolderName(string folderName)
    {
        // Arrange
        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { FolderPathKey, folderName }
        });
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);

        // Act
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<LocalFolderStoreOptions>>();

        // Assert
        Assert.Throws<OptionsValidationException>(() => options.CurrentValue);
    }

    [TestMethod]
    public void AddLocalFolderStore_Registers_Correct_Options_And_Provider()
    {
        // Arrange
        const string expectedFolderName = "SomeFolder";

        var builder = SetupBuilder(new Dictionary<string, string?>
        {
            { FolderPathKey, expectedFolderName }
        });

        // Act
        var hasFileSystem = builder.Services.Any(s =>
            s.ServiceType == typeof(IFileSystem) && s.ImplementationType == typeof(FileSystem)
        );
        var hasProvider = builder.Services.Any(s =>
            s.ServiceType == typeof(IMockResponseProvider) && s.ImplementationFactory != null
        );
        var serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<LocalFolderStoreOptions>>();

        // Assert
        Assert.IsTrue(hasFileSystem);
        Assert.IsTrue(hasProvider);
        Assert.AreEqual(options.CurrentValue.FolderPath, expectedFolderName);
    }

    [TestMethod]
    public void AddLocalFolderStore_Should_Bind_With_ConfigurationSectionPath()
    {
        // Arrange
        const string customPath = "CustomRoot:Settings";
        const string folderPath = "CustomMocks";

        var builder = SetupBuilder(
            initialData: new Dictionary<string, string?>
            {
                { $"{customPath}:FolderPath", folderPath }
            },
            configurationSectionPath: customPath
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<LocalFolderStoreOptions>>();

        // Assert
        Assert.AreEqual(folderPath, options.CurrentValue.FolderPath);
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
    public void Passed_ConfigOptions_Should_Override_Configuration()
    {
        // Arrange
        const string folderPathOverride = "AnotherFolder";
        var builder = SetupBuilder(
            initialData: new Dictionary<string, string?>
            {
                { FolderPathKey, "SomeFolder" }
            },
            configOptions: opts => opts.FolderPath = folderPathOverride
        );

        // Act
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.AreEqual(
            serviceProvider.GetRequiredService<IOptionsMonitor<LocalFolderStoreOptions>>().CurrentValue.FolderPath,
            folderPathOverride
        );
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