namespace AvaloniaExampleProject.Services.Navigation;

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
