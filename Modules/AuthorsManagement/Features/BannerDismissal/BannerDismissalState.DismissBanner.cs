using TimeWarp.State;

namespace snowcoreBlog.Frontend.AuthorsManagement.Features.BannerDismissal;

partial class BannerDismissalState
{
    public static class DismissBannerActionSet
    {
        public sealed class Action : IAction { }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {
            private BannerDismissalState BannerDismissalState => Store.GetState<BannerDismissalState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                BannerDismissalState.IsBecomeAuthorBannerDismissed = true;
                return Task.CompletedTask;
            }
        }
    }
}