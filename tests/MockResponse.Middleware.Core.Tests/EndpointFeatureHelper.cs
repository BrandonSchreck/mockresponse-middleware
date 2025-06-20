using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;

namespace MockResponse.Middleware.Core.Tests;

internal static class EndpointFeatureHelper
{
    internal static IEndpointFeature SetupEndpointFeature(Endpoint endpoint)
    {
        var endpointFeature = Substitute.For<IEndpointFeature>();
        endpointFeature.Endpoint.Returns(endpoint);
        return endpointFeature;
    }
}