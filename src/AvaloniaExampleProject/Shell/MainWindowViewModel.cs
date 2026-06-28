using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Services;
using AvaloniaExampleProject.Services.Navigation;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaExampleProject.Shell;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(Resources i18N, IThemeService themeService, INavigationService navigationService)
    {
        I18N = i18N;
        ThemeService = themeService;
        NavigationService = navigationService;
        navigationService.PropertyChanged += (_, _) => GoBackCommand.NotifyCanExecuteChanged();
    }

    public Resources I18N { get; }
    public IThemeService ThemeService { get; }
    public INavigationService NavigationService { get; }

    [RelayCommand(CanExecute = nameof(CanGoBackInternal), AllowConcurrentExecutions = false)]
    private async Task GoBackAsync(CancellationToken cancellationToken)
    {
        await NavigationService.GoBackAsync(cancellationToken);
    }

    private bool CanGoBackInternal() => NavigationService is { CanGoBack: true, IsNavigating: false };
}
