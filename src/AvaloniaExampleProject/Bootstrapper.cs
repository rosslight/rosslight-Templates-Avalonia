using System.Globalization;
using Avalonia.Controls;
using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Features.Welcome;
using AvaloniaExampleProject.Services;
using AvaloniaExampleProject.Shell;
using Darp.Utils.Assets;
using Darp.Utils.Configuration;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.FluentAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;
using JsonContext = AvaloniaExampleProject.JsonContext;

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
            // Configure the config
            .AddConfigurationFile<MainConfig>(AppDataAssets, "config.json", JsonContext.Default.MainConfig)
            // Configure localization
            .AddSingleton(Resources.Default)
            // Configure common services
            .AddCommonServices()
            // Configure Features
            .AddTransient<MainWindowViewModel>()
            .AddTransient<MainViewModel>()
            .AddWelcomeFeature()
            .AddSettingsFeature();
        return serviceCollection;
    }

    public static IServiceCollection AddSerilogLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ILogger>(provider =>
        {
            var appDataAssets = provider.GetRequiredService<IAssetsFactory>().GetReadOnlyAssets(AppDataAssets);
            ConfigService<MainConfig> configService = provider.GetRequiredService<ConfigService<MainConfig>>();
            var appInfoService = provider.GetRequiredService<IAppInformationService>();

            string logDirectory = LogExportService.GetLogDirectory(appDataAssets);
            // Using a default level here. The config service is not loaded at this point in the program.
            // After the config is loaded, the observable will be notified and the configured log level will be applied
            var loggingLevelSwitch = new LoggingLevelSwitch();
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
