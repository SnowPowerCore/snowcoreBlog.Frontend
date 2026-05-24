using System.Security.Claims;
using snowcoreBlog.Frontend.Infrastructure.Models.Auth;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions.Auth;

public static class ClaimsPrincipalMappingExtensions
{
    public static ClaimsPrincipal ToModel(this ClaimsPrincipalDto dto)
    {
        var identities = dto.Identities?.Select(x => x.ToModel()).ToList();

        if (identities is default(List<ClaimsIdentity>)) return new();

        return new(identities);
    }
}