using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;

[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class AntiforgeryState : State<AntiforgeryState>
{
    public string RequestVerificationToken { get; private set; }

    public override void Initialize()
    {
        RequestVerificationToken = string.Empty;
    }
}