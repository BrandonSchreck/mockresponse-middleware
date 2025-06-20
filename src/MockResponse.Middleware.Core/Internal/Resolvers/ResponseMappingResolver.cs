using MockResponse.Middleware.Core.Contracts;
using MockResponse.Middleware.Core.Contracts.Interfaces;
using MockResponse.Middleware.Core.Options;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MockResponse.Middleware.Core.Tests")]
namespace MockResponse.Middleware.Core.Internal.Resolvers;

/// <summary>
/// Resolves a mock reference by inspecting response metadata and configured response mappings.
/// </summary>
internal sealed class ResponseMappingResolver : IResponseMappingResolver, IDisposable
{
    private IReadOnlyDictionary<string, string> _mappings = default!;
    private readonly IDisposable? _onChangeListener;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseMappingResolver"/> class and registers for changes to <see cref="MockOptions"/>.
    /// </summary>
    /// <param name="options">The monitor for <see cref="MockOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options" /> is null.</exception>
    public ResponseMappingResolver(IOptionsMonitor<MockOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _onChangeListener = options.OnChange(OptionsChangeHandler);
        OptionsChangeHandler(options.CurrentValue);
    }

    /// <summary>
    /// Attempts to resolve a mock reference for the given metadata and optional variant.
    /// </summary>
    /// <param name="metadata">The response metadata to evaluate.</param>
    /// <param name="variant">An optional variant string used to differentiate mock mappings.</param>
    /// <param name="reference">The resolved <see cref="MockReference"/>, if found.</param>
    /// <returns><c>true</c> if a matching mock reference was found; otherwise, <c>false</c>.</returns>
    public bool TryGetMockReference(IReadOnlyList<IProducesResponseTypeMetadata> metadata, string? variant, out MockReference? reference)
    {
        foreach (var meta in metadata)
        {
            var typeName = meta.Type!.FullName;
            var key = string.IsNullOrWhiteSpace(variant) ? typeName : $"{typeName}.{variant}";

            // ReSharper disable once InvertIf
            if (_mappings.TryGetValue(key!, out var identifier))
            {
                reference = new MockReference(identifier, key!);
                return true;
            }
        }

        reference = null;
        return false;
    }

    /// <summary>
    /// Disposes the options monitor change listener if one was registered.
    /// </summary>
    public void Dispose() => _onChangeListener?.Dispose();

    /// <summary>
    /// Handles changes to <see cref="MockOptions"/> by updating the internal mapping store.
    /// </summary>
    /// <param name="options">The updated option instance.</param>
    /// <exception cref="InvalidOperationException">Thrown if the response mappings are empty.</exception>
    private void OptionsChangeHandler(MockOptions options)
    {
        _mappings = options.ResponseMappings;
        if (_mappings.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(MockOptions.ResponseMappings)} doesn't contain any mappings.");
        }
    }
}