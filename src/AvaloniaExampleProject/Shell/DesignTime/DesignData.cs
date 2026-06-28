using AvaloniaExampleProject.Assets;
using AvaloniaExampleProject.Features.Dialogs;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Features.Welcome;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject.Shell.DesignTime;

/// <summary> A static class which provides design data to views </summary>
public static class DesignData
{
    public static IServiceProvider Services => App.Current.Services;

    public static WelcomeViewModel WelcomeViewModel { get; } =
        ActivatorUtilities.CreateInstance<WelcomeViewModel>(Services);
    public static DialogsViewModel DialogsViewModel { get; } =
        ActivatorUtilities.CreateInstance<DialogsViewModel>(Services);
    public static ProjectDialogViewModel ProjectDialogViewModel { get; } =
        new(Services.GetRequiredService<Resources>());
    public static SettingsViewModel SettingsViewModel { get; } =
        ActivatorUtilities.CreateInstance<SettingsViewModel>(Services);
    public static MainViewModel MainViewModel { get; } = ActivatorUtilities.CreateInstance<MainViewModel>(Services);
}
