using TimeWarp.State;

namespace snowcoreBlog.Frontend.Articles.Features.Antiforgery;

partial class AntiforgeryState
{
    public static class SetCurrentTokenActionSet
    {
        public sealed class Action : IAction
        {
            public string? Token { get; }

            public Action(string? token)
            {
                Token = token;
            }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            private AntiforgeryState AntiforgeryState => Store.GetState<AntiforgeryState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                AntiforgeryState.RequestVerificationToken = action.Token;
                return Task.CompletedTask;
            }
        }
    }
}