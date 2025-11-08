using TimeWarp.State;

namespace snowcoreBlog.Frontend.Articles.Features.Antiforgery;

[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class ArticlesAntiforgeryState : State<ArticlesAntiforgeryState>
{
    public string RequestVerificationToken { get; private set; }

    public override void Initialize()
    {
        RequestVerificationToken = string.Empty;
    }
}