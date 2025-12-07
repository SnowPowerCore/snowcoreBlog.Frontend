using TimeWarp.State;

namespace snowcoreBlog.Frontend.Articles.Features.Antiforgery;

partial class AntiforgeryState
{
    public static class SetCurrentTokenActionSet
    {
        public sealed class Action(string? token) : IAction
        {
            public string? Token { get; } = token;
        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {

            private AntiforgeryState AntiforgeryState => Store.GetState<AntiforgeryState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                AntiforgeryState.RequestVerificationToken = action.Token;
                return Task.CompletedTask;
            }
        }
    }
}