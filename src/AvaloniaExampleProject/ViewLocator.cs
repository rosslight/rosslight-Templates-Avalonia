using Avalonia;
using Avalonia.Controls;
using AvaloniaExampleProject.Features.Settings;
using AvaloniaExampleProject.Features.Welcome;
using AvaloniaExampleProject.Shell;
using Darp.Utils.Avalonia;

namespace AvaloniaExampleProject;

/// <summary>
/// A ViewLocator class. When added to the <see cref="Application.DataTemplates"/>, ViewModels are resolved to their corresponding view
/// </summary>
public sealed class ViewLocator : ViewLocatorBase<ViewModelBase>
{
    protected override Control? Build(ViewModelBase viewModel) =>
        viewModel switch
        {
            WelcomeViewModel vm => new WelcomeView { ViewModel = vm },
            SettingsViewModel vm => new SettingsView { ViewModel = vm },
            _ => null,
        };
}
