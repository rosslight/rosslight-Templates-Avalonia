using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace AvaloniaExampleProject.Services;

public interface INotificationService
{
    /// <summary>Shows a toast notification card</summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message to be displayed in the notification.</param>
    /// <param name="type">The <see cref="T:Avalonia.Controls.Notifications.NotificationType" /> of the notification.</param>
    /// <param name="expiration">The expiry time at which the notification will close. The default time is 10 seconds.
    /// Use <see cref="F:System.TimeSpan.Zero" /> for notifications that will remain open.</param>
    void Show(string title, string message, NotificationType type, TimeSpan? expiration);
}

public sealed class MainWindowNotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;

    private WindowNotificationManager NotificationManager =>
        _notificationManager ?? throw new InvalidOperationException("Notification manager is not available yet.");

    public void SetTopLevel(TopLevel topLevel, NotificationPosition position)
    {
        _notificationManager = new WindowNotificationManager(topLevel) { Position = position };
    }

    public void Show(
        string title,
        string message,
        NotificationType type = NotificationType.Information,
        TimeSpan? expiration = null
    )
    {
        expiration ??= TimeSpan.FromSeconds(10);
        NotificationManager.Show(new Notification(title, message, type, expiration));
    }
}

public static class NotificationServiceExtensions
{
    /// <summary>Shows an informational toast notification card</summary>
    /// <param name="notificationService">The notification service to show the card with</param>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message to be displayed in the notification.</param>
    /// <param name="expiration">The expiry time at which the notification will close. The default time is 10 seconds.
    /// Use <see cref="F:System.TimeSpan.Zero" /> for notifications that will remain open.</param>
    public static void ShowInformation(
        this INotificationService notificationService,
        string title,
        string message,
        TimeSpan? expiration = null
    ) => notificationService.Show(title, message, NotificationType.Information, expiration);

    /// <summary>Shows a success toast notification card</summary>
    /// <param name="notificationService">The notification service to show the card with</param>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message to be displayed in the notification.</param>
    /// <param name="expiration">The expiry time at which the notification will close. The default time is 10 seconds.
    /// Use <see cref="F:System.TimeSpan.Zero" /> for notifications that will remain open.</param>
    public static void ShowSuccess(
        this INotificationService notificationService,
        string title,
        string message,
        TimeSpan? expiration = null
    ) => notificationService.Show(title, message, NotificationType.Success, expiration);

    /// <summary>Shows a warning toast notification card</summary>
    /// <param name="notificationService">The notification service to show the card with</param>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message to be displayed in the notification.</param>
    /// <param name="expiration">The expiry time at which the notification will close. The default time is 10 seconds.
    /// Use <see cref="F:System.TimeSpan.Zero" /> for notifications that will remain open.</param>
    public static void ShowWarning(
        this INotificationService notificationService,
        string title,
        string message,
        TimeSpan? expiration = null
    ) => notificationService.Show(title, message, NotificationType.Warning, expiration);

    /// <summary>Shows an error toast notification card</summary>
    /// <param name="notificationService">The notification service to show the card with</param>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message to be displayed in the notification.</param>
    /// <param name="expiration">The expiry time at which the notification will close. The default time is 10 seconds.
    /// Use <see cref="F:System.TimeSpan.Zero" /> for notifications that will remain open.</param>
    public static void ShowError(
        this INotificationService notificationService,
        string title,
        string message,
        TimeSpan? expiration = null
    ) => notificationService.Show(title, message, NotificationType.Error, expiration);
}
