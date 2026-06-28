using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Services;

namespace AvaloniaExampleProject.Shell;

public sealed class MainWindowViewModel(Resources i18N, IThemeService themeService) : ViewModelBase
{
    public Resources I18N { get; } = i18N;
    public IThemeService ThemeService { get; } = themeService;
}
