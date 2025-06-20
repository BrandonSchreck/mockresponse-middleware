using MockResponse.Middleware.Core.Internal.Resolvers;
using MockResponse.Middleware.Core.Options;
using MockResponse.Middleware.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace MockResponse.Middleware.Core.Tests.Internal.Resolvers;

[TestClass]
public class ResponseMappingResolverTests
{
    private class FakeClass
    {
    }

    [TestMethod]
    public void Null_Options_Throws_ArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => new ResponseMappingResolver(null!));
    }

    [TestMethod]
    public void Empty_Options_ResponseMappings_Throws_InvalidOperationException()
    {
        // Arrange
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions());

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => new ResponseMappingResolver(options));
    }

    [DataTestMethod]
    [DataRow("some-key", "some-value", null, false)]
    [DataRow("MockResponse.Middleware.Core.Tests.Internal.Resolvers.ResponseMappingResolverTests+FakeClass", "FakeClass.json", null, true)]
    [DataRow("MockResponse.Middleware.Core.Tests.Internal.Resolvers.ResponseMappingResolverTests+FakeClass.Variant", "FakeClass.Variant.json", null, false)]
    [DataRow("MockResponse.Middleware.Core.Tests.Internal.Resolvers.ResponseMappingResolverTests+FakeClass.Variant", "FakeClass.Variant.json", "Variant", true)]
    public void TryGetMockReference_Returns_Correct_Response_When_Looking_Up_References(string key, string value, string? variant, bool referenceFound)
    {
        // Arrange
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            ResponseMappings = new Dictionary<string, string>
            {
                { key, value }
            }
        });
        var resolver = new ResponseMappingResolver(options);
        var metadata = new List<IProducesResponseTypeMetadata>
        {
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(FakeClass))
        };

        // Act
        var result = resolver.TryGetMockReference(metadata, variant, out var reference);

        // Assert
        Assert.AreEqual(result, referenceFound);
        Assert.AreEqual(referenceFound ? key : null, reference?.Key);
        Assert.AreEqual(referenceFound ? value : null, reference?.Identifier);
    }

    [TestMethod]
    public void Verify_OnChange_Gets_Triggered()
    {
        // First pass should not return a reference
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            ResponseMappings = new Dictionary<string, string>
            {
                { "some-key", "some-value"  }
            }
        });
        var resolver = new ResponseMappingResolver(options);
        
        var metadata = new List<IProducesResponseTypeMetadata>
        {
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(FakeClass))
        };

        var result = resolver.TryGetMockReference(metadata, null, out var reference);
        Assert.IsFalse(result);
        Assert.IsNull(reference);

        // Updated ResponseMappings should return a reference
        options.TriggerChange(new MockOptions
        {
            ResponseMappings = new Dictionary<string, string>
            {
                { "MockResponse.Middleware.Core.Tests.Internal.Resolvers.ResponseMappingResolverTests+FakeClass", "FakeClass.json"  }
            }
        });
        result = resolver.TryGetMockReference(metadata, null, out reference);
        Assert.IsTrue(result);
        Assert.IsNotNull(reference);
    }

    [TestMethod]
    public void Dispose_Should_Remove_Disposable()
    {
        // Arrange
        var disposable = new TrackableDisposable();
        var mockOptions = new MockOptions
        {
            ResponseMappings = new Dictionary<string, string>
            {
                { "some-key", "some-value" }
            }
        };
        var options = new TestOptionsMonitor<MockOptions>(mockOptions, disposable);

        var sut = new ResponseMappingResolver(options);

        // Act
        sut.Dispose();

        // Assert
        Assert.IsTrue(disposable.WasDisposed);
    }
}