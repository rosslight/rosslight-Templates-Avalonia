using System.ComponentModel;
using AvaloniaExampleProject.Shell;

namespace AvaloniaExampleProject.Services.Navigation;

/// <summary>
/// A simple navigation service to navigate in a single-page application.
/// Assumes that all navigation is async.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    /// <summary> The page that is currently active. </summary>
    ViewModelBase? CurrentPage { get; }

    /// <summary> The route that is currently active. </summary>
    AppRoute? CurrentRoute { get; }

    /// <summary> Indicates whether the navigation stack can be navigated back to. </summary>
    bool CanGoBack { get; }

    /// <summary> Indicates whether the navigation is currently in progress. </summary>
    bool IsNavigating { get; }

    /// <summary> Resets the navigation history. </summary>
    void ResetNavigationHistory();

    /// <summary> Navigate to a page represented by an application route. </summary>
    /// <param name="cancellationToken"> The cancellationToken to cancel the navigation operation </param>
    /// <param name="route"> The route to navigate to </param>
    /// <returns> The result of the navigation </returns>
    Task<NavigationResult> NavigateToAsync(AppRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Go back to the previous page.
    /// <list type="bullet">
    /// <item>Wait for previous navigation operations to complete and take lock</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>true</c></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingFromAsync"/></item>
    /// <item>Call <see cref="IAsyncNavigationAware.OnNavigatingToAsync"/></item>
    /// <item>Pop the currently active page from the back stack</item>
    /// <item>Raise <see cref="CurrentPage"/>, <see cref="CurrentRoute"/>, <see cref="CanGoBack"/> notifications</item>
    /// <item>Set and raise <see cref="IsNavigating"/> to <c>false</c></item>
    /// <item>Release lock</item>
    /// </list>
    /// </summary>
    /// <param name="cancellationToken"> The cancellationToken to cancel the navigation operation </param>
    /// <returns> The result of the navigation </returns>
    Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default);
}
