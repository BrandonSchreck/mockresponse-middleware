using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MockResponse.Middleware.LocalFolderStore.Tests;

[TestClass]
public class LocalFolderStoreMockResponseProviderTests
{
    private IOptionsMonitor<LocalFolderStoreOptions> _optionsMock = default!;
    private IFileSystem _fileSystem = default!;

    [TestInitialize]
    public void Setup()
    {
        _fileSystem = Substitute.For<IFileSystem>();

        _optionsMock = Substitute.For<IOptionsMonitor<LocalFolderStoreOptions>>();
        _optionsMock.CurrentValue.Returns(new LocalFolderStoreOptions
        {
            FolderPath = "some/path"
        });
    }

    [TestMethod]
    public void Constructor_Throws_ArgumentNullException_For_Null_FileSystem()
    {
        // Arrange/Act/Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
            new LocalFolderStoreMockResponseProvider(null!, _optionsMock)
        );
    }

    [TestMethod]
    public void Constructor_Throws_ArgumentNullException_For_Null_LocalFolderStoreOptions()
    {
        // Arrange/Act/Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new LocalFolderStoreMockResponseProvider(_fileSystem, null!)
        );
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void Constructor_Throws_ArgumentException_For_Invalid_LocalFolderStoreOptions(string folderPath)
    {
        // Arrange
        _optionsMock.CurrentValue.Returns(new LocalFolderStoreOptions
        {
            FolderPath = folderPath
        });

        // Act/Assert
        Assert.Throws<ArgumentException>(() => new LocalFolderStoreMockResponseProvider(_fileSystem, _optionsMock));
    }

    [TestMethod]
    public async Task GetMockResponseAsync_Throws_FileNotFoundException_When_File_Does_Not_Exist()
    {
        // Arrange
        _fileSystem.Exists(Arg.Any<string>()).Throws<FileNotFoundException>();

        var sut = new LocalFolderStoreMockResponseProvider(_fileSystem, _optionsMock);

        // Act/Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => sut.GetMockResponseAsync("some-file.json"));
    }

    [TestMethod]
    public async Task GetMockResponseAsync_Returns_Response_And_Name_If_File_Exists()
    {
        // Arrange
        const string json = "{\"someProperty\":\"someValue\"}";

        _fileSystem.Exists(Arg.Any<string>()).Returns(true);
        _fileSystem.ReadAllTextAsync(Arg.Any<string>()).Returns(json);

        var sut = new LocalFolderStoreMockResponseProvider(_fileSystem, _optionsMock);

        // Act
        var (response, providerName) = await sut.GetMockResponseAsync("some-file.json");

        // Assert
        Assert.AreEqual(response, json);
        Assert.AreEqual(providerName, LocalFolderStoreMockResponseProvider.ProviderName);
    }
}