using MockResponse.Middleware.Core.Internal.Policies;
using MockResponse.Middleware.Core.Options;
using MockResponse.Middleware.TestUtilities;
using Microsoft.AspNetCore.Http;

namespace MockResponse.Middleware.Core.Tests.Internal.Policies;

[TestClass]
public class ExcludePathPolicyTests
{
    [TestMethod]
    public void Null_Options_Should_Throw_ArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => new ExcludePathPolicy(null!));
    }

    [TestMethod]
    public void Empty_MockOptions_ExcludedRequestPaths_Should_Not_Be_Bypassed()
    {
        // Arrange
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions());

        var sut = new ExcludePathPolicy(options);

        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = new PathString("/some-path")
            }
        };

        // Act
        var shouldBypass = sut.ShouldBypass(context, out var reason);

        // Assert
        Assert.IsFalse(shouldBypass);
        Assert.IsNull(reason);
    }

    [DataTestMethod]
    [DataRow("/", "/some-path", false)]
    [DataRow("/some-path", "/some-other-path", false)]
    [DataRow("/", "/", true)]
    [DataRow("/some-path", "/", true)]
    [DataRow("/some-path", "/some-path", true)]
    [DataRow("/Some-path", "/some-path", true)]
    [DataRow("/API/endpoint", "/api/", true)]
    public void ShouldBypass_Should_Bypass_If_Path_Is_Excluded(string path, string excludedPath, bool shouldBypass)
    {
        // Arrange
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            ExcludedRequestPaths = new[] { excludedPath }
        });

        var sut = new ExcludePathPolicy(options);

        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = new PathString(path)
            }
        };

        // Act
        var result = sut.ShouldBypass(context, out var reason);

        // Assert
        Assert.AreEqual(shouldBypass, result);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.AreEqual(shouldBypass, reason is not null);
    }

    [TestMethod]
    public void Verify_OnChange_Has_Been_Triggered()
    {
        // First pass should not be bypassed
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            ExcludedRequestPaths = new[] { "/pathB" }
        });

        var sut = new ExcludePathPolicy(options);

        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = new PathString("/pathA")
            }
        };

        var result = sut.ShouldBypass(context, out var reason);
        Assert.IsFalse(result);
        Assert.IsNull(reason);

        // Updated ExcludedRequestPaths should trigger onChange and bypass
        options.TriggerChange(new MockOptions
        {
            ExcludedRequestPaths = new[] { "/pathA" }
        });

        result = sut.ShouldBypass(context, out reason);
        Assert.IsTrue(result);
        Assert.IsNotNull(reason);
    }

    [TestMethod]
    public void Dispose_Should_Remove_Disposable()
    {
        // Arrange
        var disposable = new TrackableDisposable();
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions(), disposable);

        var sut = new ExcludePathPolicy(options);

        // Act
        sut.Dispose();

        // Assert
        Assert.IsTrue(disposable.WasDisposed);
    }
}