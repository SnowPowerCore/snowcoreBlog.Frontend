using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Antiforgery;

[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class ReaderAccountAntiforgeryState : State<ReaderAccountAntiforgeryState>
{
    public string RequestVerificationToken { get; private set; }

    public override void Initialize()
    {
        RequestVerificationToken = string.Empty;
    }
}