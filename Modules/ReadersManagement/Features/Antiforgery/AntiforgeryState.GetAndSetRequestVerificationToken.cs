using Apizr;
using snowcoreBlog.PublicApi.Api;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using snowcoreBlog.PublicApi.Extensions;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;

partial class AntiforgeryState
{
    public static class GetAndSetRequestVerificationTokenActionSet
    {
        public sealed class Action : IAction
        {
            public Action() { }
        }

        public sealed class Handler(IStore store, IApizrManager<IReaderAccountTokensApi> tokensApi) : ActionHandler<Action>(store)
        {

            private AntiforgeryState AntiforgeryState => Store.GetState<AntiforgeryState>();

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                using var response = await tokensApi.ExecuteAsync(static (opt, api) =>
                    api.GetAntiforgeryToken(opt), o => o.WithCancellation(cancellationToken));
                var data = response.ToData<AntiforgeryResultDto>(out var errors);
                if (data is default(AntiforgeryResultDto) && errors.Count > 0)
                    return;

                AntiforgeryState.RequestVerificationToken = data!.RequestToken!;
            }
        }
    }
}