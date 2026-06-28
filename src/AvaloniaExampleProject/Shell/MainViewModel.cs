using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Services.Navigation;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaExampleProject.Shell;

public sealed partial class MainViewModel(Resources i18N, INavigationService navigationService) : ViewModelBase
{
    public Resources I18N { get; } = i18N;
    public INavigationService NavigationService { get; } = navigationService;

    [RelayCommand]
    private async Task NavigateAsync(AppRoute route, CancellationToken cancellationToken)
    {
        await NavigationService.NavigateToAsync(route, cancellationToken);
    }
}
