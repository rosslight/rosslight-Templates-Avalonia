using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Views;
using Microsoft.Extensions.DependencyInjection;

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
            var accessor = (MainWindowStorageProviderAccessor)Services.GetRequiredService<IStorageProviderAccessor>();
            accessor.SetTopLevel(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
