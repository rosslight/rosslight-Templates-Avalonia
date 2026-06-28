using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Shell.DesignTime;
using Darp.Utils.Configuration;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AvaloniaExampleProject.Shell;

public sealed partial class MainWindow : FAAppWindow
{
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;

    public MainWindow(IServiceProvider provider)
    {
        _provider = provider;
        _logger = provider.GetRequiredService<ILogger>().ForContext<MainWindow>();
        InitializeComponent();

        DataContext = provider.GetRequiredService<MainWindowViewModel>();

        ConfigureTitleBar();

        SplashScreen = new MainAppSplashScreen(this);
    }

    [Obsolete("Should by used by designer only!")]
    public MainWindow()
        : this(DesignData.Services) { }

    internal async Task LoadAsync(CancellationToken cancellationToken)
    {
        // The first thing we should do is loading the config as most of the app's behavior depends on it
        var configurationService = _provider.GetRequiredService<ConfigService<MainConfig>>();
        var config = await configurationService.LoadConfigAsync(() => new MainConfig(), cancellationToken);

        // Initialize config-dependent services
        var i18N = _provider.GetRequiredService<Resources>();
        i18N.Culture = new CultureInfo(config.UserPreferences.SelectedLanguage);

        // Create the MainView last. Run on the current dispatcher because the LoadAsync is initialized on the threadpool
        Dispatcher.Invoke(() =>
        {
            WindowContent.Content = new MainView { ViewModel = _provider.GetRequiredService<MainViewModel>() };
        });
        _logger.Information("AvaloniaExampleProject started!");
    }

    private void ConfigureTitleBar()
    {
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.Height = 44;

        // Update colors
        _ = SubscribeToColor("ButtonForegroundDisabled", color => TitleBar.ButtonInactiveForegroundColor = color);
        _ = SubscribeToColor("AppBarButtonBackgroundPressed", color => TitleBar.ButtonPressedBackgroundColor = color);
        _ = SubscribeToColor("AppBarButtonBackgroundPointerOver", color => TitleBar.ButtonHoverBackgroundColor = color);
    }

    private IDisposable SubscribeToColor(string resourceName, Action<Color> onNext) =>
        Resources
            .GetResourceObservable(resourceName)
            .Subscribe(o =>
            {
                if (o is SolidColorBrush brush)
                    onNext(brush.Color);
            });
}

file sealed class MainAppSplashScreen(MainWindow owner) : IFAApplicationSplashScreen
{
    private readonly MainWindow _owner = owner;

    public string? AppName => null;
    public IImage AppIcon =>
        new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaExampleProject/Assets/splashscreen.png")));
    public object? SplashScreenContent => null;
    public int MinimumShowTime => 700;

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        await _owner.LoadAsync(cancellationToken);
    }
}
