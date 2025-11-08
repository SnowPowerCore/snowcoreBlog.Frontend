using Apizr;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.Articles.Features.Antiforgery;

partial class ArticlesAntiforgeryState
{
    public static class GetAndSetRequestVerificationTokenActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
        }

        public sealed class Handler : ActionHandler<Action>
        {
            private readonly IApizrManager<IArticlesTokensApi> _tokensApi;

            public Handler(IStore store, IApizrManager<IArticlesTokensApi> tokensApi) : base(store)
            {
                _tokensApi = tokensApi;
            }

            private ArticlesAntiforgeryState AntiforgeryState => Store.GetState<ArticlesAntiforgeryState>();

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                using var response = await _tokensApi.ExecuteAsync(static (opt, api) =>
                    api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
                var data = response.ToData<AntiforgeryResultDto>(out var errors);
                if (data is default(AntiforgeryResultDto) && errors.Count > 0)
                    return;

                AntiforgeryState.RequestVerificationToken = data!.RequestToken!;
            }
        }
    }
}