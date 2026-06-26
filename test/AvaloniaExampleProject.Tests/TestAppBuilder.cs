using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Models;
using AvaloniaExampleProject.Tests;
using Darp.Utils.Assets;
using Darp.Utils.Configuration;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace AvaloniaExampleProject.Tests;

public class TestAppBuilder
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyImageMagick.RegisterComparers(0.1);
        VerifyAvalonia.AddAvaloniaConvertersForAssemblyOfType<FANavigationView>();
        VerifierSettings.InitializePlugins();
    }

    private static readonly MainConfig s_mainConfig = new()
    {
        UserPreferences = new UserPreferencesConfig { SelectedLanguage = "en-EN", SelectedTheme = "Default" },
    };

    public static IServiceProvider Services { get; private set; } = null!;

    public static AppBuilder BuildAvaloniaApp()
    {
        var appInformationService = Substitute.For<IAppInformationService>();
        appInformationService.Version.Returns("1.2.3-aabbccdd");
        IServiceProvider provider = new ServiceCollection()
            .AddLogging(builder => builder.AddXUnit())
            .AddMemoryAssetsService(
                Bootstrapper.AppDataAssets,
                "base/path",
                service =>
                {
                    service.SerializeJson("config.json", s_mainConfig);
                }
            )
            .AddConfigurationFile<MainConfig>(Bootstrapper.AppDataAssets, "config.json")
            .AddAppServices()
            .AddSingleton(appInformationService)
            .AddSingleton(new Resources())
            .BuildServiceProvider();
        Services = provider;
        return AppBuilder
            .Configure(() => new App(provider))
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont()
            .AfterSetup(builder =>
            {
                if (builder.Instance is null)
                    throw new Exception("Instance is null");
                var themes = builder.Instance.Styles.OfType<FluentAvaloniaTheme>();
                builder.Instance.Styles.RemoveAll(themes);
                builder.Instance.Styles.Add(
                    new FluentAvaloniaTheme { PreferSystemTheme = false, PreferUserAccentColor = false }
                );
            });
    }
}
