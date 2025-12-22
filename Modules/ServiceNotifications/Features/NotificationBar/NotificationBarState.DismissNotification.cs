using TimeWarp.State;

namespace snowcoreBlog.Frontend.ServiceNotifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to dismiss a notification.
    /// </summary>
    public static class DismissNotificationActionSet
    {
        public sealed class Action : IAction
        {
            public Guid NotificationId { get; }

            public Action(Guid notificationId)
            {
                NotificationId = notificationId;
            }
        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {
            private NotificationBarState State => Store.GetState<NotificationBarState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                var dismissed = new HashSet<Guid>(State.DismissedNotificationIds)
                {
                    action.NotificationId
                };
                State.DismissedNotificationIds = dismissed;
                return Task.CompletedTask;
            }
        }
    }
}
