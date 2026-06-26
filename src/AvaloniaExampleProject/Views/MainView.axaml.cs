using Avalonia;
using Avalonia.Controls;
using AvaloniaExampleProject.Business;
using AvaloniaExampleProject.Models;
using AvaloniaExampleProject.ViewModels;
using Darp.Utils.Avalonia;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaExampleProject.Views;

public sealed partial class MainView : UserControlBase<MainViewModel>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;

    public MainView(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _navigationService = serviceProvider.GetRequiredService<INavigationService>();
        InitializeComponent();
    }

    [Obsolete("Should by used by designer only!")]
    public MainView()
        : this(DesignData.Services) { }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        object? firstItem = NavView.MenuItems.FirstOrDefault();
        if (firstItem is not null)
            NavView.SelectedItem = firstItem;
    }

    private async void TabStripControl_OnSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        switch (e.SelectedItemContainer?.Tag)
        {
            case Type type:
                var viewModel = (ViewModelBase)_serviceProvider.GetRequiredService(type);
                await _navigationService.NavigateToAsync(viewModel);
                break;
        }
    }
}
