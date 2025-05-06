using System.Text.Json.Serialization;
using Fido2NetLib;
using Fido2NetLib.Objects;
using static Fido2NetLib.Objects.COSE;

namespace snowcoreBlog.Frontend.Infrastructure.Context;

[JsonSerializable(typeof(AssertionOptions))]
[JsonSerializable(typeof(AuthenticatorAssertionRawResponse))]
[JsonSerializable(typeof(AuthenticatorAttestationRawResponse))]
[JsonSerializable(typeof(CredentialCreateOptions))]
[JsonSerializable(typeof(Algorithm))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true, Converters = [
    typeof(JsonStringEnumConverter<Algorithm>),
    typeof(FidoEnumConverter<Algorithm>),
    typeof(FidoEnumConverter<PublicKeyCredentialType>),
    typeof(FidoEnumConverter<AttestationConveyancePreference>),
    typeof(FidoEnumConverter<ResidentKeyRequirement>),
    typeof(FidoEnumConverter<UserVerificationRequirement>)
])]
public partial class FidoBlazorSerializerContext : JsonSerializerContext { }