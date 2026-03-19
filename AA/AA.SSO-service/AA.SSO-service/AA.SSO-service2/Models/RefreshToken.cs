using AA.SSO_service2.Entities;

namespace AA.SSO_service2.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? ReasonRevoked { get; set; }

    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;

}