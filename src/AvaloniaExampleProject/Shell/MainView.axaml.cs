using System.ComponentModel;
using Avalonia;
using AvaloniaExampleProject.Services.Navigation;
using Darp.Utils.Avalonia;
using FluentAvalonia.UI.Controls;

namespace AvaloniaExampleProject.Shell;

public sealed partial class MainView : UserControlBase<MainViewModel>
{
    private bool _isUpdatingSelection;

    public MainView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ViewModel.NavigationService.PropertyChanged += NavigationService_OnPropertyChanged;

        if (ViewModel.NavigationService.CurrentRoute is { } route)
            SelectRoute(route);
        else if (NavView.MenuItems.FirstOrDefault() is { } firstItem)
            NavView.SelectedItem = firstItem;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ViewModel.NavigationService.PropertyChanged -= NavigationService_OnPropertyChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private async void TabStripControl_OnSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (!_isUpdatingSelection && e.SelectedItemContainer?.Tag is AppRoute route)
            await ViewModel.NavigateCommand.ExecuteAsync(route);
    }

    private void NavigationService_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentRoute) && ViewModel.NavigationService.CurrentRoute is { } route)
            SelectRoute(route);
    }

    private void SelectRoute(AppRoute route)
    {
        object? selectedItem = NavView
            .MenuItems.Concat(NavView.FooterMenuItems)
            .FirstOrDefault(item => item is FANavigationViewItem { Tag: AppRoute itemRoute } && itemRoute == route);
        if (selectedItem is null || ReferenceEquals(NavView.SelectedItem, selectedItem))
            return;

        _isUpdatingSelection = true;
        try
        {
            NavView.SelectedItem = selectedItem;
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }
}
