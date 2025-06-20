using MockResponse.Middleware.Core.Internal.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace MockResponse.Middleware.Core.Tests.Internal.Resolvers;

[TestClass]
public class EndpointMetadataResolverTests
{
    [TestMethod]
    public void Should_Return_Empty_List_If_Endpoint_Is_Not_Defined()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var sut = new EndpointMetadataResolver();

        // Act
        var result = sut.GetMetadata(context, StatusCodes.Status200OK);
        
        // Assert
        Assert.IsEmpty(result);
    }

    [DataTestMethod]
    [DataRow(StatusCodes.Status200OK, 2)]
    [DataRow(StatusCodes.Status201Created, 0)]
    [DataRow(StatusCodes.Status404NotFound, 1)]
    public void Should_Return_Matching_Metadata_By_StatusCode(int statusCode, int numberOfRecords)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var sut = new EndpointMetadataResolver();

        var metadata = new List<IProducesResponseTypeMetadata>
        {
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(string)),
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(int)),
            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(IResult)),
            new ProducesResponseTypeMetadata(StatusCodes.Status404NotFound, typeof(string))
        };
        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(
            new Endpoint(null, new EndpointMetadataCollection(metadata), null)
        );
        context.Features.Set(endpointFeature);

        // Act
        var results = sut.GetMetadata(context, statusCode);

        // Assert
        Assert.AreEqual(numberOfRecords, results.Count);
        Assert.IsTrue(results.All(x => x.StatusCode == statusCode));
    }
}