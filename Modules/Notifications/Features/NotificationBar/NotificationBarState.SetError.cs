using TimeWarp.State;

namespace snowcoreBlog.Frontend.Notifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to set an error message.
    /// </summary>
    public static class SetErrorActionSet
    {
        public sealed class Action(string? errorMessage) : IAction
        {
            public string? ErrorMessage { get; } = errorMessage;
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
