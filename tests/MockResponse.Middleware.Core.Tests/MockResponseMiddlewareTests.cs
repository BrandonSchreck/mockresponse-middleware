using MockResponse.Middleware.Core.Contracts;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MockResponse.Middleware.Core.Tests;

[TestClass]
public class MockResponseMiddlewareTests
{
    private HttpContext _context = default!;
    private RequestDelegate _requestDelegate = default!;

    private IMockingPolicy _policy = default!;
    private IMockReferenceResolver _refResolver = default!;
    private IMockProviderFactory _factory = default!;
    private IMockResponseProvider _respProvider = default!;
    private ILogger<MockResponseMiddleware> _logger = default!;

    [TestInitialize]
    public void Setup()
    {
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();

        _requestDelegate = Substitute.For<RequestDelegate>();
        _requestDelegate.Invoke(Arg.Any<HttpContext>()).Returns(Task.CompletedTask);
        
        _policy = Substitute.For<IMockingPolicy>();
        _refResolver = Substitute.For<IMockReferenceResolver>();
        _factory = Substitute.For<IMockProviderFactory>();
        _respProvider = Substitute.For<IMockResponseProvider>();
        _logger = Substitute.For<ILogger<MockResponseMiddleware>>();
    }

    [TestMethod]
    public async Task InvokeAsync_Should_Bypass_Middleware_If_When_Any_Policy_Returns_True()
    {
        // Arrange
        var middleware = ConfigureMiddleware(true, "some reason");

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        await _requestDelegate.Received(1).Invoke(_context);
        _refResolver.Received(0).TryGetMockReferenceResult(_context, out Arg.Any<MockReferenceResult>());
        await _respProvider.Received(0).GetMockResponseAsync(Arg.Any<string>());
        Assert.AreEqual(StatusCodes.Status200OK, _context.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_Should_Return_404_When_MockReference_Is_Not_Found()
    {
        // Arrange
        var middleware = ConfigureMiddleware(false, null!, false);
        
        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        _policy.Received(1).ShouldBypass(_context, out Arg.Any<string>());
        _refResolver.Received(1).TryGetMockReferenceResult(_context, out Arg.Any<MockReferenceResult>());
        await _respProvider.Received(0).GetMockResponseAsync(Arg.Any<string>());
        Assert.AreEqual(StatusCodes.Status404NotFound, _context.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_Should_Return_MockReference_And_Status_Code_When_Resolver_Succeeds()
    {
        // Arrange
        const string expectedIdentifier = "someFile.json";
        const string expectedProvider = "Some Provider";
        const int expectedStatusCode = StatusCodes.Status300MultipleChoices;

        var result = MockReferenceResult.Found(new MockReference(expectedIdentifier, "SomeKey"), expectedStatusCode);

        var middleware = ConfigureMiddleware(false, null!, true, result, _ => Task.FromResult(("{\"someProperty\":\"someValue\"}", expectedProvider)));

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        _policy.Received(1).ShouldBypass(_context, out Arg.Any<string>());
        _refResolver.Received(1).TryGetMockReferenceResult(_context, out Arg.Any<MockReferenceResult>());
        await _respProvider.Received(1).GetMockResponseAsync(Arg.Any<string>());
        Assert.AreEqual(expectedStatusCode, _context.Response.StatusCode);
        Assert.AreEqual(expectedIdentifier, _context.Response.Headers["X-Mock-Identifier"]);
        Assert.AreEqual(expectedProvider, _context.Response.Headers["X-Mock-Provider"]);
    }

    [TestMethod]
    public async Task InvokeAsync_Should_Return_404_When_FileNotFoundException_Thrown()
    {
        // Arrange
        var middleware = ConfigureMiddleware(
            shouldBypass: false,
            bypassReason: null!,
            result: MockReferenceResult.Found(new MockReference("someFile.json", "some key"), StatusCodes.Status200OK),
            providerResponse: _ => Task.FromResult(("", ""))
        );

        _respProvider
            .When(provider => provider.GetMockResponseAsync(Arg.Any<string>()))
            .Do(_ => throw new FileNotFoundException());

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        _policy.Received(1).ShouldBypass(_context, out Arg.Any<string>());
        _refResolver.Received(1).TryGetMockReferenceResult(_context, out Arg.Any<MockReferenceResult>());
        await _respProvider.Received(1).GetMockResponseAsync(Arg.Any<string>());
        Assert.AreEqual(StatusCodes.Status404NotFound, _context.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_Should_Return_500_When_Unhandled_Exception_Thrown()
    {
        // Arrange
        var middleware = ConfigureMiddleware(
            shouldBypass: false,
            bypassReason: null!,
            result: MockReferenceResult.Found(new MockReference("someFile.json", "some key"), StatusCodes.Status200OK),
            providerResponse: _ => Task.FromResult(("", ""))
        );

        _respProvider
            .When(provider => provider.GetMockResponseAsync(Arg.Any<string>()))
            .Do(_ => throw new ArgumentNullException());

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        _policy.Received(1).ShouldBypass(_context, out Arg.Any<string>());
        _refResolver.Received(1).TryGetMockReferenceResult(_context, out Arg.Any<MockReferenceResult>());
        await _respProvider.Received(1).GetMockResponseAsync(Arg.Any<string>());
        Assert.AreEqual(StatusCodes.Status500InternalServerError, _context.Response.StatusCode);
    }

    private MockResponseMiddleware ConfigureMiddleware(bool shouldBypass, string? bypassReason, bool hasReferenceResult = true, MockReferenceResult? result = null, Func<string, Task<(string, string)>>? providerResponse = null)
    {
        _policy.ShouldBypass(Arg.Any<HttpContext>(), out Arg.Any<string>()).Returns(x =>
        {
            x[1] = bypassReason;
            return shouldBypass;
        });

        _refResolver.TryGetMockReferenceResult(Arg.Any<HttpContext>(), out Arg.Any<MockReferenceResult>()).Returns(x =>
        {
            x[1] = result ?? MockReferenceResult.NotFound("Not found", StatusCodes.Status404NotFound);
            return hasReferenceResult;
        });

        _respProvider.GetMockResponseAsync(Arg.Any<string>())
            .Returns(providerResponse?.Invoke("default-id") ?? Task.FromResult(("{}", "TestProvider")));

        _factory.Create().Returns(_respProvider);

        return new MockResponseMiddleware(_requestDelegate, new[] { _policy }, _refResolver, _factory, _logger);
    }
}