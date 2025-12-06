using TimeWarp.State;

namespace snowcoreBlog.Frontend.Notifications.Features.NotificationBar;

partial class NotificationBarState
{
    /// <summary>
    /// Action to set loading state.
    /// </summary>
    public static class SetLoadingActionSet
    {
        public sealed class Action : IAction
        {
            public bool IsLoading { get; }

            public Action(bool isLoading)
            {
                IsLoading = isLoading;
            }
        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {
            private NotificationBarState State => Store.GetState<NotificationBarState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                State.IsLoading = action.IsLoading;
                return Task.CompletedTask;
            }
        }
    }
}
