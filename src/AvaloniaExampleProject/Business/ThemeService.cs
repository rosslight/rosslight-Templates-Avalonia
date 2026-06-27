using Avalonia.Styling;
using AvaloniaExampleProject.Models;
using Darp.Utils.Configuration;

namespace AvaloniaExampleProject.Business;

public interface IThemeService
{
    IReadOnlyList<string> AvailableThemes { get; }
    IObservable<ThemeVariant> RequestedThemeVariant { get; }
}

public sealed class ThemeService(ConfigService<MainConfig> configurationService) : IThemeService
{
    public const string DefaultTheme = "Default";
    public const string DarkTheme = "Dark";
    public const string LightTheme = "Light";

    private readonly ConfigService<MainConfig> _configurationService = configurationService;

    public IReadOnlyList<string> AvailableThemes { get; } = [DefaultTheme, DarkTheme, LightTheme];

    public IObservable<ThemeVariant> RequestedThemeVariant =>
        _configurationService.Observe(config => ThemeVariantFromString(config.UserPreferences.SelectedTheme));

    private static ThemeVariant ThemeVariantFromString(string? theme) =>
        theme switch
        {
            DarkTheme => ThemeVariant.Dark,
            LightTheme => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };
}
