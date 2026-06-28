using Avalonia.Threading;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace AvaloniaExampleProject.Services.Navigation;

public sealed partial class NavigationService(INavigationRegistry navigationRegistry, ILogger logger)
    : ObservableObject,
        INavigationService
{
    private readonly Stack<AppRoute> _backStack = [];
    private readonly INavigationRegistry _navigationRegistry = navigationRegistry;
    private readonly ILogger _logger = logger.ForContext<NavigationService>();
    private readonly SemaphoreSlim _navigationLock = new(1, 1);
    private NavigationEntry? _currentEntry;

    public ViewModelBase? CurrentPage => _currentEntry?.ViewModel;

    public AppRoute? CurrentRoute => _currentEntry?.Route;

    public bool CanGoBack => _backStack.Count > 0;

    [ObservableProperty]
    public partial bool IsNavigating { get; private set; }

    public void ResetNavigationHistory()
    {
        _backStack.Clear();
        Dispatcher.UIThread.Invoke(() => OnPropertyChanged(nameof(CanGoBack)));
    }

    public Task<NavigationResult> NavigateToAsync(AppRoute route, CancellationToken cancellationToken = default)
    {
        return NavigateToInternalAsync(route, cancellationToken);
    }

    public async Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        Type? sourceViewModelType = _currentEntry?.ViewModelType;
        AppRoute? sourceRoute = _currentEntry?.Route;
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
                    AppRoute cancelledTargetRoute = _backStack.Peek();
                    _logger.Verbose(
                        "Navigation back from {SourceRoute} to {TargetRoute} cancelled",
                        sourceRoute,
                        cancelledTargetRoute
                    );
                    return NavigationResult.Cancelled;
                }

                AppRoute targetRoute = _backStack.Peek();
                NavigationEntry targetEntry = CreateEntry(targetRoute);
                await RunNavigatingToAsync(targetEntry.ViewModel, cancellationToken);

                _ = _backStack.Pop();
                _currentEntry = targetEntry;

                Dispatcher.UIThread.Invoke(NotifyNavigationChanged);

                _logger.Verbose(
                    "Navigated back from {SourceRoute} to {TargetRoute} ({TargetViewModelType})",
                    sourceRoute,
                    targetRoute,
                    targetEntry.ViewModelType.Name
                );
                return NavigationResult.Succeeded;
            }
            catch (OperationCanceledException e)
            {
                _logger.Verbose(e, "Navigation back from {SourceViewModelType} cancelled", sourceViewModelType?.Name);
                return NavigationResult.Cancelled;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Navigation back from {SourceViewModelType} failed", sourceViewModelType?.Name);
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

    private async Task<NavigationResult> NavigateToInternalAsync(AppRoute route, CancellationToken cancellationToken)
    {
        AppRoute? sourceRoute = _currentEntry?.Route;
        try
        {
            await _navigationLock.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.Verbose(
                "Navigation from {SourceRoute} to {TargetRoute} cancelled while waiting for lock",
                sourceRoute,
                route
            );
            return NavigationResult.Cancelled;
        }
        try
        {
            try
            {
                if (_currentEntry?.Route == route)
                {
                    _logger.Verbose(
                        "Navigation from {SourceRoute} to {TargetRoute} skipped because target is already current",
                        sourceRoute,
                        route
                    );
                    return NavigationResult.NoOp;
                }

                NavigationEntry targetEntry = CreateEntry(route);

                Dispatcher.UIThread.Invoke(() => IsNavigating = true);

                await RunNavigatingFromAsync(_currentEntry?.ViewModel, cancellationToken);

                await RunNavigatingToAsync(targetEntry.ViewModel, cancellationToken);

                if (_currentEntry is not null)
                    _backStack.Push(_currentEntry.Route);

                _currentEntry = targetEntry;
                Dispatcher.UIThread.Invoke(NotifyNavigationChanged);

                _logger.Verbose(
                    "Navigated from {SourceRoute} to {TargetRoute} ({TargetViewModelType})",
                    sourceRoute,
                    route,
                    targetEntry.ViewModelType.Name
                );
                return NavigationResult.Succeeded;
            }
            catch (OperationCanceledException e)
            {
                _logger.Verbose(e, "Navigation from {SourceRoute} to {TargetRoute} cancelled", sourceRoute, route);
                return NavigationResult.Cancelled;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Navigation from {SourceRoute} to {TargetRoute} failed", sourceRoute, route);
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
        OnPropertyChanged(nameof(CurrentRoute));
        OnPropertyChanged(nameof(CanGoBack));
    }

    private NavigationEntry CreateEntry(AppRoute route)
    {
        ViewModelBase viewModel = _navigationRegistry.Create(route);
        return new NavigationEntry(route, viewModel.GetType(), viewModel);
    }

    private sealed record NavigationEntry(AppRoute Route, Type ViewModelType, ViewModelBase ViewModel);
}
