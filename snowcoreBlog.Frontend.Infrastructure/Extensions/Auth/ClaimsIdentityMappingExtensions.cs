using System.Security.Claims;
using snowcoreBlog.Frontend.Infrastructure.Models.Auth;

namespace snowcoreBlog.Frontend.Infrastructure.Extensions.Auth;

public static class ClaimsIdentityMappingExtensions
{
    public static ClaimsIdentity ToModel(this ClaimsIdentityDto dto)
    {
        var claims = dto.Claims?.Select(x => x.ToModel()).ToList();
        var authenticationType = dto.AuthenticationType;

        if (claims is null) return new(authenticationType);

        return new(claims, authenticationType);
    }
}