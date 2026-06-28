using Avalonia.Threading;
using AvaloniaExampleProject.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace AvaloniaExampleProject.Services.Navigation;

public sealed partial class NavigationService(INavigationRegistry navigationRegistry, ILogger logger)
    : ObservableObject,
        INavigationService
{
    private readonly Stack<NavigationEntry> _backStack = [];
    private readonly INavigationRegistry _navigationRegistry = navigationRegistry;
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

    public Task<NavigationResult> NavigateToAsync(AppRoute route, CancellationToken cancellationToken = default)
    {
        ViewModelBase viewModel = _navigationRegistry.Create(route);
        return NavigateToAsync(viewModel, cancellationToken);
    }

    public Task<NavigationResult> NavigateToAsync(ViewModelBase page, CancellationToken cancellationToken = default) =>
        NavigateToInternalAsync(page, cancellationToken);

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
