using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AA.SSO_service2.Entities;
using AA.SSO_service2.Infrastructure.Data;
using AA.SSO_service2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = AA.SSO_service2.Models.RegisterRequest;

namespace AA.SSO_service2.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;


    public AuthController(
        IOptions<JwtOptions> jwtOptions,
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher)
    {
        _jwtOptions = jwtOptions.Value;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            ModelState.AddModelError(nameof(request.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            ModelState.AddModelError(nameof(request.Email), "User with this email already exists.");
            return ValidationProblem(ModelState);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            IsMfaEnabled = false
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email
        });
    }

    

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var expires = DateTime.UtcNow.AddHours(1);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var rsa = RSA.Create();
        rsa.ImportFromPem(_jwtOptions.PrivateKeyPem);

        var privateKey = new RsaSecurityKey(rsa)
        {
            KeyId = string.IsNullOrWhiteSpace(_jwtOptions.KeyId)
                ? null
                : _jwtOptions.KeyId
        };
        
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse
        {
            Token = tokenValue,
            ExpiresAtUtc = expires
        });
    }

}
