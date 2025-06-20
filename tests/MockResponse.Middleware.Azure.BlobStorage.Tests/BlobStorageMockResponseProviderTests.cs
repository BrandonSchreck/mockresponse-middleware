using MockResponse.Middleware.TestUtilities;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NSubstitute;

namespace MockResponse.Middleware.Azure.BlobStorage.Tests;

[TestClass]
public sealed class BlobStorageMockResponseProviderTests
{
    private BlobContainerClient _mockedContainer = default!;
    private IBlobContainerClientFactory _factoryMock = default!;
    private TestOptionsMonitor<BlobStorageOptions> _optionsMock = default!;

    private const string DefaultConnectionString = "connectionString";
    private const string DefaultContainerName = "containerName";

    [TestInitialize]
    public void Setup()
    {
        _mockedContainer = Substitute.For<BlobContainerClient>();
        _mockedContainer
            .Exists(Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(true, Substitute.For<Response>()));

        _factoryMock = Substitute.For<IBlobContainerClientFactory>();
        _factoryMock
            .Create(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_mockedContainer);

        _optionsMock = new TestOptionsMonitor<BlobStorageOptions>(new BlobStorageOptions
        {
            ConnectionString = DefaultConnectionString,
            ContainerName = DefaultContainerName
        });
    }

    [TestMethod]
    public void Constructor_Throws_ArgumentNullException_For_Null_IBlobContainerClientFactory()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => new BlobStorageMockResponseProvider(null!, _optionsMock));
    }

    [TestMethod]
    public void Constructor_Throws_ArgumentNullException_For_Null_BlobStorageOptions()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => new BlobStorageMockResponseProvider(_factoryMock, null!));
    }

    [DataTestMethod]
    [DataRow(null, "container")]
    [DataRow("", "container")]
    [DataRow("connectionString", null)]
    [DataRow("connectionString", "")]
    public async Task Constructor_Throws_ArgumentException_For_Invalid_BlobStorageOptions(string connectionString, string container)
    {
        // Arrange
        _optionsMock = new TestOptionsMonitor<BlobStorageOptions>(new BlobStorageOptions
        {
            ConnectionString = connectionString,
            ContainerName = container
        });

        // Act
        var provider = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.GetMockResponseAsync("fake.json"));
    }

    [TestMethod]
    public async Task Constructor_Throws_InvalidOperationException_When_Container_Does_Not_Exist()
    {
        // Arrange
        _mockedContainer
            .When(c => c.Exists(Arg.Any<CancellationToken>()))
            .Do(_ => throw new RequestFailedException("womp womp"));

        // Act
        var provider = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetMockResponseAsync("fake.json"));
        _mockedContainer.Received(1).Exists(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public void BlobContainerClientFactory_Create_Should_Not_Be_Called_During_Initialization()
    {
        // Arrange


        // Act
        var sut = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Assert
        _factoryMock.Received(0).Create(Arg.Any<string>(), Arg.Any<string>());
        _mockedContainer.Received(0).Exists(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetMockResponseAsync_Throws_FileNotFound_Exception_When_ExistAsync_Is_False()
    {
        // Arrange
        const string identifier = "some-file.json";

        var mockedBlobClient = SetupBlobClient(identifier);
        _factoryMock.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockedContainer);

        var sut = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Act/Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => sut.GetMockResponseAsync(identifier));
        _mockedContainer.Received(1).Exists(Arg.Any<CancellationToken>());
        _mockedContainer.Received(1).GetBlobClient(identifier);
        await mockedBlobClient.Received(1).ExistsAsync(Arg.Any<CancellationToken>());
        await mockedBlobClient.Received(0).DownloadContentAsync();
    }

    [TestMethod]
    public async Task GetMockResponseAsync_Returns_Content_When_ExistsAsync_Is_True()
    {
        // Arrange
        const string identifier = "some-file.json";
        const string json = "{\"someProperty\":\"someValue\"}";

        var mockedBlobClient = SetupBlobClient(identifier, json);
        _factoryMock.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockedContainer);

        var sut = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Act
        var (response, providerName) = await sut.GetMockResponseAsync(identifier);

        // Assert
        Assert.AreEqual(json, response);
        Assert.AreEqual(providerName, BlobStorageMockResponseProvider.ProviderName);
        _mockedContainer.Received(1).Exists(Arg.Any<CancellationToken>());
        _mockedContainer.Received(1).GetBlobClient(identifier);
        await mockedBlobClient.Received(1).ExistsAsync(Arg.Any<CancellationToken>());
        await mockedBlobClient.Received(1).DownloadContentAsync();
    }

    [TestMethod]
    public async Task Changes_To_Options_Should_Create_A_New_IBlobContainerClientFactory()
    {
        // Arrange
        const string identifier = "fake.json";
        const string newConnectionString = "newConnString";
        const string newContainerName = "newContName";

        var mockedBlobClient = SetupBlobClient(identifier);
        _factoryMock.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockedContainer);

        var sut = new BlobStorageMockResponseProvider(_factoryMock, _optionsMock);

        // Act
        await Assert.ThrowsAsync<FileNotFoundException>(() => sut.GetMockResponseAsync(identifier));

        _optionsMock.TriggerChange(new BlobStorageOptions
        {
            ConnectionString = newConnectionString,
            ContainerName = newContainerName
        });

        await Assert.ThrowsAsync<FileNotFoundException>(() => sut.GetMockResponseAsync(identifier));

        // Assert
        _factoryMock.Received(1).Create(DefaultConnectionString, DefaultContainerName);
        _factoryMock.Received(1).Create(newConnectionString, newContainerName);
    }

    [TestMethod]
    public void Dispose_Should_Remove_OptionsMonitor_ChangeListener()
    {
        // Arrange
        var trackableDisposable = new TrackableDisposable();
        var options = new BlobStorageOptions
        {
            ConnectionString = DefaultConnectionString,
            ContainerName = DefaultContainerName
        };
        var optionsMonitor = new TestOptionsMonitor<BlobStorageOptions>(options, trackableDisposable);
        var provider = new BlobStorageMockResponseProvider(_factoryMock, optionsMonitor);

        // Act
        provider.Dispose();

        // Assert
        Assert.IsNotNull(provider);
        Assert.IsTrue(trackableDisposable.WasDisposed);
    }

    private BlobClient CreateBlobClient(string identifier)
    {
        var mockedBlobClient = Substitute.For<BlobClient>();
        _mockedContainer.GetBlobClient(identifier).Returns(mockedBlobClient);
        return mockedBlobClient;
    }

    private BlobClient SetupBlobClient(string identifier, bool exists = false)
    {
        var mockedBlobClient = CreateBlobClient(identifier);
        mockedBlobClient
            .ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(exists, Substitute.For<Response>())));

        return mockedBlobClient;
    }

    private BlobClient SetupBlobClient(string identifier, string json)
    {
        var result = BlobsModelFactory.BlobDownloadResult(BinaryData.FromString(json));
        var mockedResponse = Substitute.For<Response<BlobDownloadResult>>();
        mockedResponse.Value.Returns(result);

        var mockedBlobClient = SetupBlobClient(identifier, true);
        mockedBlobClient.DownloadContentAsync().Returns(mockedResponse);
        return mockedBlobClient;
    }
}