using TimeWarp.State;

namespace snowcoreBlog.Frontend.AuthorsManagement.Features.BannerDismissal;

[PersistentState(PersistentStateMethod.LocalStorage)]
public sealed partial class BannerDismissalState : State<BannerDismissalState>
{
    public bool IsBecomeAuthorBannerDismissed { get; private set; }

    public override void Initialize()
    {
        IsBecomeAuthorBannerDismissed = false;
    }
}
