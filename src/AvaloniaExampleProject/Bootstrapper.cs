using System.Globalization;
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
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

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
            .AddSingleton<ILogExportService, LogExportService>()
            // Configure ViewModels
            .AddTransient<MainWindowViewModel>()
            .AddTransient<MainViewModel>()
            .AddTransient<WelcomeViewModel>()
            .AddTransient<SettingsViewModel>();
        return serviceCollection;
    }

    private static IServiceCollection AddLocalization(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton(Resources.Default);

    public static IServiceCollection AddSerilogLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ILogger>(provider =>
        {
            var appDataAssets = provider.GetRequiredService<IAssetsFactory>().GetReadOnlyAssets(AppDataAssets);
            IConfigurationService<MainConfig> configService = provider.GetRequiredService<
                IConfigurationService<MainConfig>
            >();
            var appInfoService = provider.GetRequiredService<IAppInformationService>();

            string logDirectory = Path.Join(appDataAssets.BasePath, "logs");
            var loggingLevelSwitch = new LoggingLevelSwitch(configService.Config.Diagnostics.LogLevel);
            _ = configService.Observe(x => x.Diagnostics.LogLevel).Subscribe(x => loggingLevelSwitch.MinimumLevel = x);

            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application.Name", "AvaloniaExampleApp")
                .Enrich.WithProperty("Application.MachineName", Environment.MachineName)
                .Enrich.WithProperty("Application.SessionId", appInfoService.SessionId)
                .WriteTo.Async(configuration =>
                {
                    configuration.Console(formatProvider: CultureInfo.InvariantCulture);
                    if (Design.IsDesignMode)
                        return;
                    Directory.CreateDirectory(logDirectory);
                    configuration.File(
                        new CompactJsonFormatter(),
                        Path.Combine(logDirectory, "AvaloniaExampleApp-.clef"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true
                    );
                })
                .CreateLogger();
        });
    }
}
