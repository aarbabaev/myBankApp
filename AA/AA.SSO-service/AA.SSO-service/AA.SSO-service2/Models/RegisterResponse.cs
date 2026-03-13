namespace AA.SSO_service2.Models;

public sealed class RegisterResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}