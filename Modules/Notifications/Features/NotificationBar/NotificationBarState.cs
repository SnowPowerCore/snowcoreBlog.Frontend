using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Notifications.Features.NotificationBar;

/// <summary>
/// State for managing active notifications displayed in the top bar.
/// </summary>
public sealed partial class NotificationBarState : State<NotificationBarState>
{
    /// <summary>
    /// List of active notifications to display.
    /// </summary>
    public IReadOnlyList<NotificationDto> ActiveNotifications { get; private set; } = [];

    /// <summary>
    /// Set of notification IDs that have been dismissed by the user.
    /// </summary>
    public IReadOnlySet<Guid> DismissedNotificationIds { get; private set; } = new HashSet<Guid>();

    /// <summary>
    /// Whether notifications are currently being loaded.
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets notifications that should be displayed (active and not dismissed).
    /// </summary>
    public IEnumerable<NotificationDto> VisibleNotifications =>
        ActiveNotifications.Where(n => !DismissedNotificationIds.Contains(n.Id));

    /// <summary>
    /// Whether there are any visible notifications.
    /// </summary>
    public bool HasVisibleNotifications => VisibleNotifications.Any();

    public override void Initialize()
    {
        ActiveNotifications = [];
        DismissedNotificationIds = new HashSet<Guid>();
        IsLoading = false;
        ErrorMessage = null;
    }
}
