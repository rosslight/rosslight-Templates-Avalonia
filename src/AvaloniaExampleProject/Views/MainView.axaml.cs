using Avalonia;
using Avalonia.Controls;
using AvaloniaExampleProject.Models;
using AvaloniaExampleProject.ViewModels;
using Darp.Utils.Avalonia;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AvaloniaExampleProject.Views;

public sealed partial class MainView : UserControlBase<MainViewModel>
{
    private readonly ILogger<MainView> _logger;
    private readonly FANavigationTransitionInfo _transitionInfo = new FASuppressNavigationTransitionInfo();

    public MainView(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<MainView>>();
        InitializeComponent();
        MainFrame.NavigationPageFactory = new DependencyInjectionPageFactory(serviceProvider);
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

    private void TabStripControl_OnSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        switch (e.SelectedItemContainer?.Tag)
        {
            case Type type:
                MainFrame.Navigate(type, null, _transitionInfo);
                break;
        }
    }

    private void MainFrame_OnNavigationFailed(object sender, FANavigationFailedEventArgs e)
    {
        _logger.LogError(
            e.Exception,
            "Navigation to {Type} has failed because of {Message}",
            e.SourcePageType,
            e.Exception.Message
        );
    }
}

file sealed class DependencyInjectionPageFactory(IServiceProvider serviceProvider) : IFANavigationPageFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public Control GetPage(Type srcType)
    {
        return new UserControl { Content = _serviceProvider.GetRequiredService(srcType) };
    }

    public Control GetPageFromObject(object target) => throw new NotSupportedException();
}
