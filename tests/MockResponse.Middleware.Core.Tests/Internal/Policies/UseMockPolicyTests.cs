using MockResponse.Middleware.Core.Internal.Policies;
using MockResponse.Middleware.Core.Options;
using MockResponse.Middleware.TestUtilities;

namespace MockResponse.Middleware.Core.Tests.Internal.Policies;

[TestClass]
public class UseMockPolicyTests
{
    [TestMethod]
    public void UseMockPolicy_Should_Throw_Exception_If_Options_Is_Null()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => new UseMockPolicy(null!));
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ShouldBypass_If_UseMock_Is_True(bool useMock)
    {
        // Arrange
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            UseMock = useMock
        });
            
        var sut = new UseMockPolicy(options);

        // Act
        var result = sut.ShouldBypass(null!, out var reason);

        // Assert
        Assert.AreEqual(useMock, !result);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.AreEqual(useMock, reason is null);
    }

    [TestMethod]
    public void Verify_OnChange_Has_Been_Triggered()
    {
        // Fist pass should be false (not bypass)
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions
        {
            UseMock = true
        });

        var sut = new UseMockPolicy(options);

        Assert.IsFalse(sut.ShouldBypass(null!, out _));

        // After update, should be true (should bypass)
        options.TriggerChange(new MockOptions
        {
            UseMock = false
        });

        Assert.IsTrue(sut.ShouldBypass(null!, out _));
    }

    [TestMethod]
    public void Dispose_Should_Remove_Disposable()
    {
        // Arrange
        var disposable = new TrackableDisposable();
        var options = new TestOptionsMonitor<MockOptions>(new MockOptions(), disposable);

        var sut = new UseMockPolicy(options);

        // Act
        sut.Dispose();

        // Assert
        Assert.IsTrue(disposable.WasDisposed);
    }
}