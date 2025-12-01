namespace snowcoreBlog.Frontend.Infrastructure.Models.Auth;

public class ClaimDto
{
    public required string Type { get; set; }

    public required string Value { get; set; }

    public string? ValueType { get; set; }

    public string? Issuer { get; set; }

    public string? OriginalIssuer { get; set; }
}