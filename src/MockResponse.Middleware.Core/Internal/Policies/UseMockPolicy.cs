using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Policies;

/// <summary>
/// A policy that determines whether mock responses should be bypassed
/// based on the <see cref="MockOptions.UseMock"/> setting.
/// </summary>
internal sealed class UseMockPolicy : IMockingPolicy, IDisposable
{
    private bool _useMock;
    private readonly IDisposable? _onChangeListener;

    /// <summary>
    /// Initializes a new instance of the <see cref="UseMockPolicy"/> class
    /// and registers for configuration change notifications.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="MockOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
    public UseMockPolicy(IOptionsMonitor<MockOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _onChangeListener = options.OnChange(OptionsChangeHandler);
        OptionsChangeHandler(options.CurrentValue);
    }

    /// <summary>
    /// Evaluates whether mocking should be bypassed based on the current configuration.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="reason">Outputs the reason mocking is being bypassed, if applicable.</param>
    /// <returns><c>true</c> if mocking should be bypassed; otherwise, <c>false</c>.</returns>
    public bool ShouldBypass(HttpContext context, out string reason)
    {
        if (!_useMock)
        {
            reason = "Mocking disabled";
            return true;
        }

        reason = null!;
        return false;
    }

    /// <summary>
    /// Disposes of the options change listener to prevent memory leaks.
    /// </summary>
    public void Dispose() => _onChangeListener?.Dispose();

    /// <summary>
    /// Handles updates to the <see cref="MockOptions"/> configuration.
    /// </summary>
    /// <param name="options">The updated options.</param>
    private void OptionsChangeHandler(MockOptions options)
    {
        _useMock = options.UseMock;
    }
}