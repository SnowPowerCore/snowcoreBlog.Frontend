using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Register;

partial class RegisterState
{
    public static class SetCurrentInputActionSet
    {
        public sealed class Action : IAction
        {
            public RequestCreateReaderAccountDto? Request { get; }

            public Action(RequestCreateReaderAccountDto? request)
            {
                Request = request;
            }

        }

        public sealed class Handler(IStore store) : ActionHandler<Action>(store)
        {

            private RegisterState RegisterState => Store.GetState<RegisterState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                RegisterState.CurrentModel = action.Request;
                return Task.CompletedTask;
            }
        }
    }
}