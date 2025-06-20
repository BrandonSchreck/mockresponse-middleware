using MockResponse.Middleware.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace MockResponse.Middleware.Core.Tests.Extensions;

[TestClass]
public class HttpContextExtensionsTests
{
    [TestMethod]
    public async Task WriteResponseAsync_With_Status_Code_Writes_Correct_Plain_Text_Response_Body()
    {
        // Arrange
        const string expectedResponse = "something";
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        // Act
        await context.WriteResponseAsync(expectedResponse, StatusCodes.Status201Created);

        // Assert
        var responseBody = await GetResponseBodyAsText(context.Response);
        Assert.AreEqual(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
        Assert.AreEqual(expectedResponse, responseBody);
    }

    [TestMethod]
    public async Task WriteResponseAsync_With_Identifier_And_Provider_Writes_Correct_Headers_Response_Body()
    {
        // Arrange
        const string identifier = "mock-id";
        const string provider = "LocalFolder";
        const string responseBody = "{\"someProp\":\"someValue\"}";

        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        // Act
        await context.WriteResponseAsync(identifier, provider, responseBody, StatusCodes.Status201Created);

        // Assert
        var body = await GetResponseBodyAsText(context.Response);
        Assert.AreEqual(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.AreEqual("application/json", context.Response.ContentType);
        Assert.AreEqual(identifier, context.Response.Headers["X-Mock-Identifier"]);
        Assert.AreEqual(provider, context.Response.Headers["X-Mock-Provider"]);
        Assert.AreEqual(responseBody, body);
    }

    private static async Task<string> GetResponseBodyAsText(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(response.Body).ReadToEndAsync();
    }
}