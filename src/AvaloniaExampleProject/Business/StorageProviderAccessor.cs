using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvaloniaExampleProject.Business;

/// <summary> An interface for accessing the <see cref="IStorageProvider"/> of the application. </summary>
public interface IStorageProviderAccessor
{
    /// <summary>
    /// Gets the <see cref="IStorageProvider"/> of the application used for file pickers and bookmarks.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the storage provider is not available yet.</exception>
    IStorageProvider StorageProvider { get; }
}

public sealed class MainWindowStorageProviderAccessor : IStorageProviderAccessor
{
    private TopLevel? _topLevel;

    public IStorageProvider StorageProvider =>
        _topLevel?.StorageProvider ?? throw new InvalidOperationException("Storage provider is not available yet.");

    public void SetTopLevel(TopLevel topLevel) => _topLevel = topLevel;
}
