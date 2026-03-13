namespace AA.SSO_service2.Models;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string KeyId { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public string PublicKeyPem { get; set; } = string.Empty;
}
