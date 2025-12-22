using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ServiceNotifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to set the active notifications.
    /// </summary>
    public static class SetNotificationsActionSet
    {
        public sealed class Action : IAction
        {
            public IReadOnlyList<NotificationDto> Notifications { get; }

            public Action(IReadOnlyList<NotificationDto> notifications)
            {
                Notifications = notifications;
            }
        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {
            private NotificationBarState State => Store.GetState<NotificationBarState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                State.ActiveNotifications = action.Notifications;
                State.IsLoading = false;
                State.ErrorMessage = null;
                return Task.CompletedTask;
            }
        }
    }
}