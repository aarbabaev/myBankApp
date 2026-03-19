using System.ComponentModel.DataAnnotations;

namespace AA.SSO_service2.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    [MaxLength(256)]
    public string? ReplacedByTokenHash { get; set; }
    public string? ReasonRevoked { get; set; }
    [MaxLength(39)]
    public string? CreatedByIp { get; set; }
    [MaxLength(39)]
    public string? RevokedByIp { get; set; }
    [MaxLength]
    public string? UserAgent { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; set; } = null!;
    
    

    



    
}