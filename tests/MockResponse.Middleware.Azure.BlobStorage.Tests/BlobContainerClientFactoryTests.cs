using System.Collections.Concurrent;
using System.Reflection;
using Azure.Storage.Blobs;

namespace MockResponse.Middleware.Azure.BlobStorage.Tests;

internal static class ConcurrentDictionaryTestHelpers
{
    public static ConcurrentDictionary<BlobClientCacheKey, BlobContainerClient> GetInternalCache(BlobContainerClientFactory factory)
    {
        var fld = typeof(BlobContainerClientFactory).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("_cache not found");
        return (ConcurrentDictionary<BlobClientCacheKey, BlobContainerClient>)fld.GetValue(factory)!;
    }
}

[TestClass]
public sealed class BlobContainerClientFactoryTests
{
    private const string ConnectionString1 = "UseDevelopmentStorage=true";
    private const string ConnectionString2 = "UseDevelopmentStorage=true";
    private const string ContainerName1 = "A";
    private const string ContainerName2 = "B";

    private ConcurrentDictionary<BlobClientCacheKey, BlobContainerClient> _cache = default!;
    private BlobContainerClientFactory _factory = default!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new BlobContainerClientFactory();
        _cache = ConcurrentDictionaryTestHelpers.GetInternalCache(_factory);
    }

    [TestMethod]
    public void Create_Should_Return_The_BlobContainerClient_And_Only_Contain_One_Item()
    {
        // Arrange/Act
        var container1 = _factory.Create(ConnectionString1, ContainerName1);
        var container2 = _factory.Create(ConnectionString1, ContainerName1);

        // Assert
        Assert.AreSame(container1, container2);
        Assert.AreEqual(1, _cache.Count);
        CollectionAssert.AreEquivalent(
            new[] { new BlobClientCacheKey(ConnectionString1, ContainerName1) }.ToList(),
            _cache.Keys.ToList()
        );
    }

    [TestMethod]
    public void Create_With_Different_Configuration_Should_Clear_Previous_BlobContainerClient()
    {
        // Arrange/Act
        var container1 = _factory.Create(ConnectionString1, ContainerName1);
        var container2 = _factory.Create(ConnectionString2, ContainerName2);

        // Assert
        Assert.AreNotSame(container1, container2);
        Assert.AreEqual(1, _cache.Count);
        CollectionAssert.AreEquivalent(
            new[] { new BlobClientCacheKey(ConnectionString2, ContainerName2) }.ToList(),
            _cache.Keys.ToList()
        );
    }

    [TestMethod]
    public void Create_Should_Only_Add_One_Entry_Concurrently()
    {
        // Arrange
        var bag = new ConcurrentBag<BlobContainerClient>();

        // Act
        Parallel.For(0, 20, _ => bag.Add(_factory.Create(ConnectionString1, ContainerName1)));

        // Assert
        var distinct = bag.Distinct().ToList();
        Assert.AreEqual(1, distinct.Count);
        Assert.AreEqual(1, _cache.Count);
    }
}