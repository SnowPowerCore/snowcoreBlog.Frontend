using TimeWarp.State;

namespace snowcoreBlog.Frontend.ServiceNotifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to set an error message.
    /// </summary>
    public static class SetErrorActionSet
    {
        public sealed class Action : IAction
        {
            public string? ErrorMessage { get; }

            public Action(string? errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {
            private NotificationBarState State => Store.GetState<NotificationBarState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                State.ErrorMessage = action.ErrorMessage;
                State.IsLoading = false;
                return Task.CompletedTask;
            }
        }
    }
}
