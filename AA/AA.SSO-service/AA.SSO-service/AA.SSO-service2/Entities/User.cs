using System.ComponentModel.DataAnnotations;

namespace AA.SSO_service2.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; } 
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}