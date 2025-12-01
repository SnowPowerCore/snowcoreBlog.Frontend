namespace snowcoreBlog.Frontend.Infrastructure.Models.Auth;

public class ClaimsPrincipalDto
{
    public ICollection<ClaimsIdentityDto>? Identities { get; set; }
}