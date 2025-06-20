namespace MockResponse.Middleware.Core.Contracts.Interfaces;

/// <summary>
/// Defines a contract that enforces each mock response provider to declare a unique logical name identifier.
/// </summary>
public interface IMockResponseProviderDefinition
{
    /// <summary>
    /// Gets the logical name of the mock response provider. This name is used for configuration and identification purposes.
    /// </summary>
    static abstract string ProviderName { get; }
}