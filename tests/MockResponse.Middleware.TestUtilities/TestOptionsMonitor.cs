using Microsoft.Extensions.Options;

namespace MockResponse.Middleware.TestUtilities;

public class TestOptionsMonitor<T>(T initial, IDisposable? disposable = null) : IOptionsMonitor<T>
{
    public T CurrentValue { get; private set; } = initial;

    private readonly IDisposable _disposable = disposable ?? new NoOpDisposable();
    private readonly List<Action<T, string>> _listeners = new();
    
    public T Get(string? name) => CurrentValue;

    public IDisposable OnChange(Action<T, string> listener)
    {
        _listeners.Add(listener);
        return _disposable;
    }

    public void TriggerChange(T newValue, string name = "")
    {
        CurrentValue = newValue;
        foreach (var listener in _listeners)
        {
            listener(newValue, name);
        }
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}