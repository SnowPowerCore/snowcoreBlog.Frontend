using Apizr;
using Microsoft.AspNetCore.Antiforgery;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.SharedComponents.Features.Antiforgery;

partial class AntiforgeryState
{
    public static class GetAndSetRequestVerificationTokenActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly IApizrManager<ITokensApi> _tokensApi;

            public Handler(IStore store, IApizrManager<ITokensApi> tokensApi) : base(store)
            {
                _tokensApi = tokensApi;
            }

            private AntiforgeryState AntiforgeryState => Store.GetState<AntiforgeryState>();

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                var response = await _tokensApi.ExecuteAsync(static (opt, api) =>
                    api.GetAntiforgeryToken(opt));
                var data = response.ToData<AntiforgeryTokenSet>(out var errors);
                if (data is not default(AntiforgeryTokenSet) && errors.Count == 0)
                {
                    AntiforgeryState.RequestVerificationToken = data!.RequestToken;
                }
            }
        }
    }
}