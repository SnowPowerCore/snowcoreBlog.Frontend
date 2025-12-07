using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Notifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to set the active notifications.
    /// </summary>
    public static class SetNotificationsActionSet
    {
        public sealed class Action(IReadOnlyList<NotificationDto> notifications) : IAction
        {
            public IReadOnlyList<NotificationDto> Notifications { get; } = notifications;
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