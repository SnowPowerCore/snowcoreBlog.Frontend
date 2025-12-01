namespace snowcoreBlog.Frontend.Infrastructure.Models.Auth;

public class ClaimsIdentityDto
{
    public string? AuthenticationType { get; set; }

    public ICollection<ClaimDto>? Claims { get; set; }
}