using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaExampleProject.ViewModels;

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
