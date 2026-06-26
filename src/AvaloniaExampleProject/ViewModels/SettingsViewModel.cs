using System.Globalization;
using AsyncAwaitBestPractices;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Assets;
using Darp.Utils.Configuration;
using Darp.Utils.Dialog;
using Serilog;
using Serilog.Events;

namespace AvaloniaExampleProject.ViewModels;

public sealed partial class SettingsViewModel(
    Resources i18N,
    IConfigurationService<MainConfig> configurationService,
    IDialogService dialogService,
    IAppInformationService appInformationService,
    IAssetsFactory assetService,
    ILogExportService logExportService,
    ILogger logger
) : ViewModelBase
{
    private readonly IConfigurationService<MainConfig> _configurationService = configurationService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IAppInformationService _appInformationService = appInformationService;
    private readonly IReadOnlyAssetsService _appDataService = assetService.GetReadOnlyAssets(
        Bootstrapper.AppDataAssets
    );
    private readonly ILogExportService _logExportService = logExportService;
    private readonly ILogger _logger = logger.ForContext<SettingsViewModel>();

    public static LogEventLevel[] AvailableLogEvents { get; } =
    [LogEventLevel.Verbose, LogEventLevel.Debug, LogEventLevel.Information];

    public Resources I18N { get; } = i18N;
    public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
    [
        new(ThemeService.DefaultTheme, i18N.Observe(x => x.Settings_Theme_Default)),
        new(ThemeService.DarkTheme, i18N.Observe(x => x.Settings_Theme_Dark)),
        new(ThemeService.LightTheme, i18N.Observe(x => x.Settings_Theme_Light)),
    ];
    public IObservable<string> AppVersion =>
        I18N.Observe(x => x.FormatSettings_About_Version(_appInformationService.Version));
    public IObservable<string> AppSessionId =>
        I18N.Observe(x => x.FormatSettings_About_SessionId(_appInformationService.SessionId));
    public IObservable<string> ConfigurationLocation =>
        I18N.Observe(x => x.FormatSettings_About_ConfigLocation($"{_appDataService.BasePath}/config.json"));

    [ObservableProperty]
    public partial CultureInfo SelectedLanguage { get; set; } =
        Resources.GetCulture(configurationService.Config.UserPreferences.SelectedLanguage);

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = configurationService.Config.UserPreferences.SelectedTheme;

    [ObservableProperty]
    public partial LogEventLevel SelectedLogEventLevel { get; set; } = configurationService.Config.Diagnostics.LogLevel;

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

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    partial void OnSelectedLogEventLevelChanged(LogEventLevel value)
    {
        SaveConfig(c => c with { Diagnostics = new DiagnosticsConfig(LogLevel: value) });
    }

    [RelayCommand]
    private async Task ExportLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var storageProvider = App.GetStorageProvider();
            var storageFile = await storageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions { SuggestedFileName = CreateLogExportFileName() }
            );
            if (storageFile is null)
                return;
            await using var destinationStream = await storageFile.OpenWriteAsync();
            await _logExportService.ExportAsync(destinationStream, cancellationToken);
            _logger.Debug("Saved logs to {Path}", storageFile.Path);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Could not save logs because of {Message}", e.Message);
        }
    }

    private static string CreateLogExportFileName() => $"AvaloniaExampleApp-logs-{DateTime.Now:yyyyMMdd}.zip";

    [RelayCommand]
    private async Task ShowLicensesDialogAsync(CancellationToken cancellationToken)
    {
        string title = I18N.Settings_About_LibrariesTitle;
        await using var contentStream = AssetLoader.Open(
            new Uri("avares://AvaloniaExampleProject/Assets/THIRD-PARTY-NOTICES.txt")
        );
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

    private void SaveConfig(Func<MainConfig, MainConfig> createConfig)
    {
        Task.Run(async () =>
            {
                var currentConfig = _configurationService.Config;
                var newConfig = createConfig(currentConfig);
                if (currentConfig == newConfig)
                    return;
                await _configurationService.WriteConfigurationAsync(newConfig);
                _logger
                    .ForContext("OldConfig", currentConfig)
                    .ForContext("NewConfig", newConfig)
                    .Verbose("Saved new configuration");
            })
            .SafeFireAndForget(e => _logger.Error(e, "Could not save configuration because of {Message}", e.Message));
    }
}

public sealed record ThemeOption(string Key, IObservable<string> DisplayName);
