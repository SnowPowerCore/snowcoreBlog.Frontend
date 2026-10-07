using System.Text.Json.Serialization;
using snowcoreBlog.PublicApi.BusinessObjects.Dto;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ReadersManagement.Features.Register;

[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class RegisterState : State<RegisterState>
{
    [JsonInclude]
    public RequestCreateReaderAccountDto CurrentModel { get; private set; } =
        new() { Email = string.Empty, FirstName = string.Empty, NickName = string.Empty, InitialEmailConsent = false, ConfirmedAgreement = false };

    public override void Initialize()
    {
        CurrentModel =
            new() { Email = string.Empty, FirstName = string.Empty, NickName = string.Empty, InitialEmailConsent = false, ConfirmedAgreement = false };
    }
}