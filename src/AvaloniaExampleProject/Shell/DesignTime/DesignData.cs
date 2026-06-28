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

    public static WelcomeViewModel WelcomeViewModel => ActivatorUtilities.CreateInstance<WelcomeViewModel>(Services);
    public static DialogsViewModel DialogsViewModel => ActivatorUtilities.CreateInstance<DialogsViewModel>(Services);
    public static ProjectDialogViewModel ProjectDialogViewModel => new(Services.GetRequiredService<Resources>());
    public static SettingsViewModel SettingsViewModel => ActivatorUtilities.CreateInstance<SettingsViewModel>(Services);
    public static MainViewModel MainViewModel => ActivatorUtilities.CreateInstance<MainViewModel>(Services);
}
