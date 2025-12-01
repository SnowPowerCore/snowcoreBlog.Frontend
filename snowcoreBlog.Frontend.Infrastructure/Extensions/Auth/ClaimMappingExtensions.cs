using System.Security.Claims;
using snowcoreBlog.Frontend.Infrastructure.Models.Auth;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions.Auth;

public static class ClaimMappingExtensions
{
    public static Claim ToModel(this ClaimDto dto) =>
        new(dto.Type, dto.Value, dto.ValueType, dto.Issuer, dto.OriginalIssuer);
}