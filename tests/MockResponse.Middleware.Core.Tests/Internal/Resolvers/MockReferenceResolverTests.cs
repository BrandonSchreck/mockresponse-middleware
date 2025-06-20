using MockResponse.Middleware.Core.Contracts;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Internal.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using NSubstitute;

namespace MockResponse.Middleware.Core.Tests.Internal.Resolvers;

[TestClass]
public class MockReferenceResolverTests
{
    private readonly Endpoint _endpoint = new(null, null, "some-name");

    [TestMethod]
    public void Missing_MockStatus_Header_Returns_NotFound_MockReferenceResult()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var (metadataResolver, mappingResolver) = SetupDependencyInjection();
        var sut = new MockReferenceResolver(metadataResolver, mappingResolver);

        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(_endpoint);
        context.Features.Set(endpointFeature);

        // Act
        var result = sut.TryGetMockReferenceResult(context, out var selectionResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsInstanceOfType<MockReferenceResult>(selectionResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, selectionResult.StatusCode);
        Assert.AreEqual("Missing required 'X-Mock-Status' header.", selectionResult.ErrorMessage);
    }

    [TestMethod]
    public void Invalid_MockStatus_Returns_NotFound_MockReferenceResult()
    {
        // Arrange
        const string invalidStatusCode = "invalid-status-code";

        var context = new DefaultHttpContext();
        context.Request.Headers.Append("x-mock-status", invalidStatusCode);

        var (metadataResolver, mappingResolver) = SetupDependencyInjection();
        var sut = new MockReferenceResolver(metadataResolver, mappingResolver);

        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(_endpoint);
        context.Features.Set(endpointFeature);

        // Act
        var result = sut.TryGetMockReferenceResult(context, out var selectionResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsInstanceOfType<MockReferenceResult>(selectionResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, selectionResult.StatusCode);
        Assert.AreEqual($"'{invalidStatusCode}' is not a valid StatusCode.", selectionResult.ErrorMessage);
    }

    [TestMethod]
    public void Missing_Metadata_Returns_NotFound_MockReferenceResult()
    {
        // Arrange
        const int statusCode = 200;
        const string endpointName = "some-name";

        var context = new DefaultHttpContext();
        context.Request.Headers.Append("x-mock-status", statusCode.ToString());

        var (metadataResolver, mappingResolver) = SetupDependencyInjection(new List<IProducesResponseTypeMetadata>());
        var sut = new MockReferenceResolver(metadataResolver, mappingResolver);

        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(_endpoint);
        context.Features.Set(endpointFeature);

        // Act
        var result = sut.TryGetMockReferenceResult(context, out var selectionResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsInstanceOfType<MockReferenceResult>(selectionResult);
        Assert.AreEqual(StatusCodes.Status501NotImplemented, selectionResult.StatusCode);
        Assert.AreEqual($"No [{statusCode}] status code metadata was found for endpoint [{endpointName}]", selectionResult.ErrorMessage);
    }

    [TestMethod]
    public void Undefined_MockReference_Returns_NotFound_MockReferenceResult()
    {
        // Arrange
        const int statusCode = 200;
        const string endpointName = "some-name";
        const string variant = "some-variant";

        var context = new DefaultHttpContext();
        context.Request.Headers.Append("x-mock-status", statusCode.ToString());
        context.Request.Headers.Append("x-mock-variant", variant);

        var metadata = new List<IProducesResponseTypeMetadata>
        {
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(string))
        };
        var (metadataResolver, mappingResolver) = SetupDependencyInjection(metadata, hasMockReference: false, mockReference: null!);
        var sut = new MockReferenceResolver(metadataResolver, mappingResolver);

        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(_endpoint);
        context.Features.Set(endpointFeature);

        // Act
        var result = sut.TryGetMockReferenceResult(context, out var selectionResult);

        // Assert
        Assert.IsFalse(result);
        Assert.IsInstanceOfType<MockReferenceResult>(selectionResult);
        Assert.AreEqual(StatusCodes.Status404NotFound, selectionResult.StatusCode);
        Assert.AreEqual($"No [{statusCode}] status code mapping was found for endpoint [{endpointName}] and variant [{variant}]", selectionResult.ErrorMessage);
    }

    [TestMethod]
    public void Defined_MockReference_Returns_Found_MockReferenceResult()
    {
        // Arrange
        const int expectedStatusCode = StatusCodes.Status200OK;
        var mockReference = new MockReference("identifier", "key");

        var context = new DefaultHttpContext();
        context.Request.Headers.Append("x-mock-status", expectedStatusCode.ToString());

        var metadata = new List<IProducesResponseTypeMetadata>
        {
            new ProducesResponseTypeMetadata(expectedStatusCode, typeof(string))
        };
        var (metadataResolver, mappingResolver) = SetupDependencyInjection(metadata, hasMockReference: true, mockReference);
        var sut = new MockReferenceResolver(metadataResolver, mappingResolver);

        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(_endpoint);
        context.Features.Set(endpointFeature);

        // Act
        var result = sut.TryGetMockReferenceResult(context, out var selectionResult);

        // Assert
        Assert.IsTrue(result);
        Assert.IsInstanceOfType<MockReferenceResult>(selectionResult);
        Assert.IsNull(selectionResult.ErrorMessage);
        Assert.AreEqual(expectedStatusCode, selectionResult.StatusCode);
        Assert.AreSame(selectionResult.Reference, mockReference);
    }

    private static (IEndpointMetadataResolver metadataResolver, IResponseMappingResolver mappingResolver) SetupDependencyInjection() =>
    (
        Substitute.For<IEndpointMetadataResolver>(),
        Substitute.For<IResponseMappingResolver>()
    );

    private static (IEndpointMetadataResolver metadataResolver, IResponseMappingResolver mappingResolver) SetupDependencyInjection(IReadOnlyList<IProducesResponseTypeMetadata> metadataList)
    {
        var (metadataResolver, mappingResolver) = SetupDependencyInjection();
        metadataResolver
            .GetMetadata(Arg.Any<HttpContext>(), Arg.Any<int>())
            .Returns(metadataList);

        return (metadataResolver, mappingResolver);
    }

    private static (IEndpointMetadataResolver metadataResolver, IResponseMappingResolver mappingResolver) SetupDependencyInjection(IReadOnlyList<IProducesResponseTypeMetadata> metadataList, bool hasMockReference, MockReference mockReference)
    {
        var (metadataResolver, mappingResolver) = SetupDependencyInjection(metadataList);
        mappingResolver
            .TryGetMockReference(Arg.Any<IReadOnlyList<IProducesResponseTypeMetadata>>(), Arg.Any<string?>(), out Arg.Any<MockReference>()!)
            .Returns(call =>
            {
                call[2] = mockReference;
                return hasMockReference;
            });

        return (metadataResolver, mappingResolver);
    }
}