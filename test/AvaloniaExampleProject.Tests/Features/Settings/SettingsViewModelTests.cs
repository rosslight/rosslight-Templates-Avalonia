using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia.Headless.XUnit;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Services;
using Darp.Utils.Assets;
using Darp.Utils.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static AvaloniaExampleProject.Tests.VerifyHelpers;

namespace AvaloniaExampleProject.Tests.Features.Settings;

public class SettingsViewModelTests
{
    private readonly IAssetsService _configAssetsService;
    private readonly ConfigService<MainConfig> _configurationService;
    private readonly ServiceProvider _services;

    public SettingsViewModelTests()
    {
        _services = new ServiceCollection()
            .AddMemoryAssetsService(Bootstrapper.AppDataAssets, "path/to/config")
            .AddConfigurationFile<MainConfig>(Bootstrapper.AppDataAssets, "config.json")
            .AddAppServices()
            .AddSingleton(Serilog.Core.Logger.None)
            .AddLogging()
            .BuildServiceProvider();
        _configAssetsService = _services.GetRequiredService<IAssetsFactory>().GetAssets(Bootstrapper.AppDataAssets);
        _configurationService = _services.GetRequiredService<ConfigService<MainConfig>>();
    }

    [AvaloniaFact]
    public Task Render()
    {
        var control = new SettingsView
        {
            ViewModel = ActivatorUtilities.CreateInstance<SettingsViewModel>(TestAppBuilder.Services),
        };
        return VerifyControl(control).ScrubMembersWithType<Resources>();
    }

    [AvaloniaFact]
    public Task Render_AboutExpanded()
    {
        var control = new SettingsView
        {
            ViewModel = ActivatorUtilities.CreateInstance<SettingsViewModel>(TestAppBuilder.Services),
            AboutSettingsExpander = { IsExpanded = true },
        };
        return VerifyControl(control).ScrubMembersWithType<Resources>();
    }

    [Fact(Timeout = 1000)]
    public async Task ThemeChange()
    {
        const string newTheme = ThemeService.LightTheme;
        var tempConfig = new MainConfig(new UserPreferencesConfig("en", ThemeService.DarkTheme));
        await _configurationService.LoadConfigAsync(
            () => tempConfig,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var viewModel = ActivatorUtilities.CreateInstance<SettingsViewModel>(_services);
        var themeService = _services.GetRequiredService<IThemeService>();

        var themeTask = themeService.RequestedThemeVariant.Skip(1).FirstAsync().ToTask();
        viewModel.SelectedTheme = newTheme;
        var observedThemeVariant = await themeTask;

        observedThemeVariant.Key.ShouldBe(newTheme);
        _configurationService.Config.UserPreferences.SelectedTheme.ShouldBe(newTheme);
        viewModel.SelectedTheme.ShouldBe(newTheme);
    }

    [Fact(Timeout = 1000)]
    public async Task LanguageChange()
    {
        var newLanguage = CultureInfo.GetCultureInfo(Resources.Languages.German);
        var tempConfig = new MainConfig(new UserPreferencesConfig("en", ThemeService.DarkTheme));
        await _configurationService.LoadConfigAsync(
            () => tempConfig,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var viewModel = ActivatorUtilities.CreateInstance<SettingsViewModel>(_services);
        var i18N = _services.GetRequiredService<Resources>();
        i18N.Culture = new CultureInfo(Resources.Languages.Default);

        var configTask = _configurationService
            .Observe(x => x.UserPreferences.SelectedLanguage)
            .Skip(1)
            .FirstAsync()
            .ToTask();
        viewModel.SelectedLanguage = newLanguage;
        string observedLanguage = await configTask;

        i18N.Settings_Title.ShouldBe("Einstellungen");
        observedLanguage.ShouldBe(newLanguage.Name);
        _configurationService.Config.UserPreferences.SelectedLanguage.ShouldBe(newLanguage.Name);
        viewModel.SelectedLanguage.ShouldBe(newLanguage);
    }
}
