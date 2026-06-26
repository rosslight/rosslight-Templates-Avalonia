using System.ComponentModel;
using Avalonia.Threading;
using AvaloniaExampleProject.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AvaloniaExampleProject.Business;

public enum NavigationResult
{
    Succeeded,
    Cancelled,
    Failed,
    NoOp,
}

/// <summary>
/// A simple navigation service to navigate in a single-page application.
/// Assumes that all navigation is async.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    /// <summary> The page that is currently active. </summary>
    ViewModelBase? CurrentPage { get; }

    /// <summary> The type of the page that is currently active. </summary>
    Type? CurrentPageType { get; }

    /// <summary> Indicates whether the navigation stack can be navigated back to. </summary>
    bool CanGoBack { get; }

    /// <summary> Indicates whether the navigation is currently in progress. </summary>
    bool IsNavigating { get; }

    /// <summary> Resets the navigation history. </summary>
    void ResetNavigationHistory();

    /// <summary> Navigate to a page of type TViewModel. The page will be resolved from the service provider. </summary>
    /// <param name="cancellationToken"> The cancellationToken to cancel the navigation operation </param>
    /// <typeparam name="TViewModel"> The type of the viewModel </typeparam>
    /// <returns> The result of the navigation </returns>
    Task<NavigationResult> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigate to a specific page
    /// <list type="bullet">
    /// <item>Wait for previous navigation operations to complete and take lock</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>true</c></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingFromAsync"/></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingToAsync"/></item>
    /// <item>Add the new page to the navigation stack</item>
    /// <item>Raise <see cref="CurrentPage"/>, <see cref="CurrentPageType"/>, <see cref="CanGoBack"/> notifications</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>false</c></item>
    /// <item>Release lock</item>
    /// </list>
    /// </summary>
    /// <param name="page"> The page to navigate to </param>
    /// <param name="cancellationToken"> The cancellationToken to cancel the navigation operation </param>
    /// <typeparam name="TViewModel"> The type of the viewModel </typeparam>
    /// <returns> The result of the navigation </returns>
    Task<NavigationResult> NavigateToAsync<TViewModel>(TViewModel page, CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Go back to the previous page.
    /// <list type="bullet">
    /// <item>Wait for previous navigation operations to complete and take lock</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>true</c></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingFromAsync"/></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingToAsync"/></item>
    /// <item>Pop the currently active page from the back stack</item>
    /// <item>Raise <see cref="CurrentPage"/>, <see cref="CurrentPageType"/>, <see cref="CanGoBack"/> notifications</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>false</c></item>
    /// <item>Release lock</item>
    /// </list>
    /// </summary>
    /// <param name="cancellationToken"> The cancellationToken to cancel the navigation operation </param>
    /// <returns> The result of the navigation </returns>
    Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default);
}

public interface IAsyncNavigationAware
{
    /// <summary> Called when the navigation stack is popped and the current page is no longer the active page. </summary>
    /// <param name="cancellationToken"> The cancellation token to cancel the navigation operation </param>
    /// <returns>A task that represents the navigation in progress</returns>
    Task OnNavigatingFromAsync(CancellationToken cancellationToken);

    /// <summary> Called when the current page is navigated to and before it becomes active. </summary>
    /// <param name="cancellationToken"> The cancellation token to cancel the navigation operation </param>
    /// <returns>A task that represents the navigation in progress</returns>
    Task OnNavigatingToAsync(CancellationToken cancellationToken);
}

public sealed partial class NavigationService(IServiceProvider serviceProvider, ILogger logger)
    : ObservableObject,
        INavigationService
{
    private readonly Stack<NavigationEntry> _backStack = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger _logger = logger.ForContext<NavigationService>();
    private readonly SemaphoreSlim _navigationLock = new(1, 1);
    private NavigationEntry? _currentEntry;

    public ViewModelBase? CurrentPage => _currentEntry?.ViewModel;

    public Type? CurrentPageType => _currentEntry?.ViewModelType;

    public bool CanGoBack => _backStack.Count > 0;

    [ObservableProperty]
    public partial bool IsNavigating { get; private set; }

    public void ResetNavigationHistory()
    {
        _backStack.Clear();
        Dispatcher.UIThread.Invoke(() => OnPropertyChanged(nameof(CanGoBack)));
    }

    public Task<NavigationResult> NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : ViewModelBase
    {
        ViewModelBase viewModel = (ViewModelBase)_serviceProvider.GetRequiredService(typeof(TViewModel));
        return NavigateToInternalAsync(viewModel, cancellationToken);
    }

    public Task<NavigationResult> NavigateToAsync<TViewModel>(
        TViewModel page,
        CancellationToken cancellationToken = default
    )
        where TViewModel : ViewModelBase => NavigateToInternalAsync(page, cancellationToken);

    public async Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        Type? sourceViewModelType = _currentEntry?.ViewModelType;
        try
        {
            await _navigationLock.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.Verbose(
                "Navigation back from {SourceViewModelType} cancelled while waiting for lock",
                sourceViewModelType?.Name
            );
            return NavigationResult.Cancelled;
        }
        try
        {
            try
            {
                if (_backStack.Count == 0)
                {
                    _logger.Verbose(
                        "Navigation back skipped because there is no previous page for {SourceViewModelType}",
                        sourceViewModelType?.Name
                    );
                    return NavigationResult.NoOp;
                }

                Dispatcher.UIThread.Invoke(() => IsNavigating = true);

                await RunNavigatingFromAsync(_currentEntry?.ViewModel, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    Type targetViewModelType = _backStack.Peek().ViewModelType;
                    _logger.Verbose(
                        "Navigation back from {SourceViewModelType} to {TargetViewModelType} cancelled",
                        sourceViewModelType?.Name,
                        targetViewModelType.Name
                    );
                    return NavigationResult.Cancelled;
                }

                NavigationEntry targetEntry = _backStack.Peek();
                await RunNavigatingToAsync(targetEntry.ViewModel, cancellationToken);

                _currentEntry = _backStack.Pop();

                Dispatcher.UIThread.Invoke(NotifyNavigationChanged);

                _logger.Verbose(
                    "Navigated back from {SourceViewModelType} to {TargetViewModelType}",
                    sourceViewModelType?.Name,
                    targetEntry.ViewModelType.Name
                );
                return NavigationResult.Succeeded;
            }
            catch (OperationCanceledException e)
            {
                _logger.Verbose(e, "Navigation back from {SourceViewModelType} cancelled", sourceViewModelType?.Name);
                return NavigationResult.Cancelled;
            }
            catch (Exception exception)
            {
                _logger.Error(
                    exception,
                    "Navigation back from {SourceViewModelType} failed",
                    sourceViewModelType?.Name
                );
                return NavigationResult.Failed;
            }
            finally
            {
                Dispatcher.UIThread.Invoke(() => IsNavigating = false);
            }
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    private async Task<NavigationResult> NavigateToInternalAsync(
        ViewModelBase viewModel,
        CancellationToken cancellationToken
    )
    {
        Type targetViewModelType = viewModel.GetType();
        Type? sourceViewModelType = _currentEntry?.ViewModelType;
        try
        {
            await _navigationLock.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.Verbose(
                "Navigation from {SourceViewModelType} to {TargetViewModelType} cancelled while waiting for lock",
                sourceViewModelType?.Name,
                targetViewModelType.Name
            );
            return NavigationResult.Cancelled;
        }
        try
        {
            try
            {
                if (_currentEntry?.ViewModelType == targetViewModelType)
                {
                    _logger.Verbose(
                        "Navigation from {SourceViewModelType} to {TargetViewModelType} skipped because target is already current",
                        sourceViewModelType?.Name,
                        targetViewModelType.Name
                    );
                    return NavigationResult.NoOp;
                }

                Dispatcher.UIThread.Invoke(() => IsNavigating = true);

                await RunNavigatingFromAsync(_currentEntry?.ViewModel, cancellationToken);

                NavigationEntry targetEntry = new(targetViewModelType, viewModel);
                await RunNavigatingToAsync(targetEntry.ViewModel, cancellationToken);

                if (_currentEntry is not null)
                    _backStack.Push(_currentEntry);

                _currentEntry = targetEntry;
                Dispatcher.UIThread.Invoke(NotifyNavigationChanged);

                _logger.Verbose(
                    "Navigated from {SourceViewModelType} to {TargetViewModelType}",
                    sourceViewModelType?.Name,
                    targetViewModelType.Name
                );
                return NavigationResult.Succeeded;
            }
            catch (OperationCanceledException e)
            {
                _logger.Verbose(
                    e,
                    "Navigation from {SourceViewModelType} to {TargetViewModelType} cancelled",
                    sourceViewModelType?.Name,
                    targetViewModelType.Name
                );
                return NavigationResult.Cancelled;
            }
            catch (Exception exception)
            {
                _logger.Error(
                    exception,
                    "Navigation from {SourceViewModelType} to {TargetViewModelType} failed",
                    sourceViewModelType?.Name,
                    targetViewModelType.Name
                );
                return NavigationResult.Failed;
            }
            finally
            {
                Dispatcher.UIThread.Invoke(() => IsNavigating = false);
            }
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    private static async Task RunNavigatingFromAsync(ViewModelBase? viewModel, CancellationToken cancellationToken)
    {
        if (viewModel is IAsyncNavigationAware navigationAware)
            await navigationAware.OnNavigatingFromAsync(cancellationToken);
    }

    private static async Task RunNavigatingToAsync(ViewModelBase viewModel, CancellationToken cancellationToken)
    {
        if (viewModel is IAsyncNavigationAware navigationAware)
            await navigationAware.OnNavigatingToAsync(cancellationToken);
    }

    private void NotifyNavigationChanged()
    {
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(CurrentPageType));
        OnPropertyChanged(nameof(CanGoBack));
    }

    private sealed record NavigationEntry(Type ViewModelType, ViewModelBase ViewModel);
}
