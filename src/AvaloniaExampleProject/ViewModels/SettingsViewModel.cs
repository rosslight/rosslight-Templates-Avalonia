using System.Globalization;
using AsyncAwaitBestPractices;
using Avalonia.Platform;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Configuration;
using Darp.Utils.Dialog;
using Microsoft.Extensions.Logging;

namespace AvaloniaExampleProject.ViewModels;

public sealed partial class SettingsViewModel(
    Resources i18N,
    IConfigurationService<MainConfig> configurationService,
    IDialogService dialogService,
    IAppInformationService appInformationService,
    ILogger<SettingsViewModel> logger
) : ViewModelBase
{
    private readonly IConfigurationService<MainConfig> _configurationService = configurationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IAppInformationService _appInformationService = appInformationService;
    private readonly ILogger<SettingsViewModel> _logger = logger;
    public Resources I18N { get; } = i18N;
    public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
    [
        new(ThemeService.DefaultTheme, i18N.Observe(x => x.Settings_Theme_Default)),
        new(ThemeService.DarkTheme, i18N.Observe(x => x.Settings_Theme_Dark)),
        new(ThemeService.LightTheme, i18N.Observe(x => x.Settings_Theme_Light)),
    ];
    public IObservable<string> AppVersion =>
        I18N.Observe(x => x.FormatSettings_About_Version(_appInformationService.Version));

    [ObservableProperty]
    public partial CultureInfo SelectedLanguage { get; set; } =
        Resources.GetCulture(configurationService.Config.UserPreferences.SelectedLanguage);

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = configurationService.Config.UserPreferences.SelectedTheme;

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    partial void OnSelectedLanguageChanged(CultureInfo value)
    {
        if (value is null)
            return;
        I18N.Culture = value;
        SaveConfig(c => c with { UserPreferences = c.UserPreferences with { SelectedLanguage = value.Name } });
    }

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    partial void OnSelectedThemeChanged(string value)
    {
        if (value is null)
            return;
        SaveConfig(c => c with { UserPreferences = c.UserPreferences with { SelectedTheme = value } });
    }

    private void SaveConfig(Func<MainConfig, MainConfig> createConfig)
    {
        Task.Run(async () =>
            {
                var currentConfig = _configurationService.Config;
                var newConfig = createConfig(currentConfig);
                await _configurationService.WriteConfigurationAsync(newConfig);
            })
            .SafeFireAndForget(e =>
                _logger.LogError(e, "Could not save configuration because of {Message}", e.Message)
            );
    }

    [RelayCommand]
    private async Task ShowLicensesDialogAsync(CancellationToken cancellationToken)
    {
        string title = I18N.Settings_About_LibrariesTitle;
        await using var contentStream = AssetLoader.Open(new Uri("avares://AvaloniaExampleProject/Assets/NOTICE.md"));
        using var reader = new StreamReader(contentStream);
        string content = await reader.ReadToEndAsync(cancellationToken);
        await _dialogService.CreateMessageBoxDialog(title, content, true).ShowAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task ShowChangelogDialogAsync(CancellationToken cancellationToken)
    {
        string title = I18N.Settings_About_ChangelogTitle;
        await using var contentStream = AssetLoader.Open(
            new Uri("avares://AvaloniaExampleProject/Assets/CHANGELOG.md")
        );
        using var reader = new StreamReader(contentStream);
        string content = await reader.ReadToEndAsync(cancellationToken);
        await _dialogService.CreateMessageBoxDialog(title, content, true).ShowAsync(cancellationToken);
    }
}

public sealed record ThemeOption(string Key, IObservable<string> DisplayName);
