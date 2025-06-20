namespace MockResponse.Middleware.TestUtilities;

public class TrackableDisposable : IDisposable
{
    public bool WasDisposed { get; private set; }

    public void Dispose()
    {
        WasDisposed = true;
    }
}