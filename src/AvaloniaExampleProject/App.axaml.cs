using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using AvaloniaExampleProject.Views;

namespace AvaloniaExampleProject;

public sealed class App : Application
{
    public App(IServiceProvider provider)
    {
        Current = this;
        Services = provider;
        DataTemplates.Add(new ViewLocator());
    }

    /// <summary> The service provider which is injected on app start </summary>
    public IServiceProvider Services { get; }

    /// <summary> The current instance of the app. </summary>
    /// <exception cref="InvalidOperationException"> Thrown if the app was not created yet </exception>
    [field: MaybeNull, AllowNull]
    public static new App Current
    {
        get => field ?? throw new InvalidOperationException("Not created yet!");
        private set;
    }

    /// <summary>
    /// Gets the storage provider of the current application. This can be used for FilePicker operations or similar
    /// </summary>
    /// <returns> The storage provider </returns>
    /// <exception cref="InvalidOperationException"> Thrown if the app was not started with a valid lifetime or is not initialized yet. </exception>
    public static IStorageProvider GetStorageProvider()
    {
        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
            return desktop.MainWindow.StorageProvider;
        throw new InvalidOperationException("Storage provider is not available! Could not find a desktop MainWindow");
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
#if DEBUG
            this.AttachDeveloperTools(options =>
            {
                options.Gesture = new KeyGesture(Key.F11);
            });
#endif
            desktop.MainWindow = new MainWindow(Services);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
