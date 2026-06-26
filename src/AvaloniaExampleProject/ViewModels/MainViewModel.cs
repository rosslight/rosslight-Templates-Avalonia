using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;

namespace AvaloniaExampleProject.ViewModels;

public sealed class MainViewModel(Resources i18N, INavigationService navigationService) : ViewModelBase
{
    public Resources I18N { get; } = i18N;
    public INavigationService NavigationService { get; } = navigationService;
}
