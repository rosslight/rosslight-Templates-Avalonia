using Avalonia.Controls;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Models;
using AvaloniaExampleProject.ViewModels;
using Darp.Utils.Assets;
using Darp.Utils.Configuration;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.FluentAvalonia;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject;

public static class Bootstrapper
{
    public const string AppDataAssets = "AppDataAssets";

    public static IServiceCollection AddAppServices(this IServiceCollection serviceCollection)
    {
        if (Design.IsDesignMode)
            // Avoid actual edits when using the designer
            serviceCollection.AddMemoryAssetsService(AppDataAssets, "AvaloniaExampleProject");
        else
            serviceCollection.AddAppDataAssetsService(AppDataAssets, "AvaloniaExampleProject");
        serviceCollection
            // Configure core services
            .AddSingleton<IDialogService, AvaloniaDialogService>()
            .AddConfigurationFile<MainConfig>(AppDataAssets, "config.json", JsonContext.Default.MainConfig)
            .AddLocalization()
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<IAppInformationService, AppInformationService>()
            // Configure ViewModels
            .AddTransient<MainWindowViewModel>()
            .AddTransient<MainViewModel>()
            .AddTransient<WelcomeViewModel>()
            .AddTransient<SettingsViewModel>();
        return serviceCollection;
    }

    private static IServiceCollection AddLocalization(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton(Resources.Default);
}
