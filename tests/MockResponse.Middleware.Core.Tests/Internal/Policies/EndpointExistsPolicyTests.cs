using MockResponse.Middleware.Core.Internal.Policies;
using Microsoft.AspNetCore.Http;

namespace MockResponse.Middleware.Core.Tests.Internal.Policies;

[TestClass]
public class EndpointExistsPolicyTests
{
    [TestMethod]
    public void Undefined_Endpoint_Should_Bypass()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var sut = new EndpointExistsPolicy();

        // Act
        var shouldBypass = sut.ShouldBypass(context, out var reason);

        // Assert
        Assert.IsTrue(shouldBypass);
        Assert.IsNotNull(reason);
    }

    [TestMethod]
    public void Defined_Endpoint_Should_Not_Bypass()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var endpointFeature = EndpointFeatureHelper.SetupEndpointFeature(
            new Endpoint(null, null, null)
        );
        context.Features.Set(endpointFeature);

        var sut = new EndpointExistsPolicy();

        // Act
        var shouldBypass = sut.ShouldBypass(context, out var reason);

        // Assert
        Assert.IsFalse(shouldBypass);
        Assert.IsNull(reason);
    }
}