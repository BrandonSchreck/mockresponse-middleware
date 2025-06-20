namespace MockResponse.Middleware.LocalFolderStore;

/// <summary>
/// Abstraction for file system operations used in mock response providers.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The full path of the file to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool Exists(string path);

    /// <summary>
    /// Asynchronously reads all text from the specified file.
    /// </summary>
    /// <param name="path">The full path of the file to check.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the file's contents as a string.
    /// </returns>
    Task<string> ReadAllTextAsync(string path);
}

/// <summary>
/// Default implementation of <see cref="IFileSystem"/> using <see cref="System.IO"/>.
/// </summary>
public class FileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool Exists(string path) => File.Exists(path);

    /// <inheritdoc />
    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
}