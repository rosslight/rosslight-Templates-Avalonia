using AvaloniaExampleProject.Features.Dialogs;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Features.Welcome;
using AvaloniaExampleProject.Services.Navigation;
using AvaloniaExampleProject.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject;

public enum AppRoute
{
    Welcome,
    Dialogs,
    Settings,
}

public sealed class NavigationRegistry(IServiceProvider serviceProvider) : INavigationRegistry
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ViewModelBase Create(AppRoute route) =>
        route switch
        {
            AppRoute.Welcome => _serviceProvider.GetRequiredService<WelcomeViewModel>(),
            AppRoute.Dialogs => _serviceProvider.GetRequiredService<DialogsViewModel>(),
            AppRoute.Settings => _serviceProvider.GetRequiredService<SettingsViewModel>(),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, null),
        };
}
