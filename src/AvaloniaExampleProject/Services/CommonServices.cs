using AvaloniaExampleProject.Services.Navigation;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.FluentAvalonia;
using Microsoft.Extensions.DependencyInjection;
using NavigationService = AvaloniaExampleProject.Services.Navigation.NavigationService;

namespace AvaloniaExampleProject.Services;

public static class CommonServices
{
    public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IDialogService, AvaloniaDialogService>()
            // Add common services
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<INavigationRegistry, NavigationRegistry>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IAppInformationService, AppInformationService>()
            .AddSingleton<IStorageProviderAccessor, MainWindowStorageProviderAccessor>()
            .AddSingleton<ILogExportService, LogExportService>();
    }
}
