using Avalonia;
using Darp.Utils.Avalonia;
using FluentAvalonia.UI.Controls;

namespace AvaloniaExampleProject.Shell;

public sealed partial class MainView : UserControlBase<MainViewModel>
{
    public MainView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        object? firstItem = NavView.MenuItems.FirstOrDefault();
        if (firstItem is not null)
            NavView.SelectedItem = firstItem;
    }

    private async void TabStripControl_OnSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItemContainer?.Tag is AppRoute route)
            await ViewModel.NavigateCommand.ExecuteAsync(route);
    }
}
