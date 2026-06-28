using AvaloniaExampleProject.Features.Dialogs;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Features.Welcome;
using AvaloniaExampleProject.Services.Navigation;
using AvaloniaExampleProject.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject;

public abstract record AppRoute
{
    private AppRoute() { }

    public static AppRoute Welcome { get; } = new WelcomeRoute();
    public static AppRoute Dialogs { get; } = new DialogsRoute();
    public static AppRoute Settings { get; } = new SettingsRoute();

    public sealed record WelcomeRoute : AppRoute;

    public sealed record DialogsRoute : AppRoute;

    public sealed record SettingsRoute : AppRoute;
}

public sealed class NavigationRegistry(IServiceProvider serviceProvider) : INavigationRegistry
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ViewModelBase Create(AppRoute route) =>
        route switch
        {
            AppRoute.WelcomeRoute => ActivatorUtilities.CreateInstance<WelcomeViewModel>(_serviceProvider),
            AppRoute.DialogsRoute => ActivatorUtilities.CreateInstance<DialogsViewModel>(_serviceProvider),
            AppRoute.SettingsRoute => ActivatorUtilities.CreateInstance<SettingsViewModel>(_serviceProvider),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, null),
        };
}
