using System.Security.Cryptography;
using AA.SSO_service2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AA.SSO_service2.Controllers;

[ApiController]
[AllowAnonymous]
[Route(".well-known")]
public sealed class WellKnownController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;

    public WellKnownController(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    [HttpGet("jwks.json")]
    public ActionResult<JwksResponse> GetJwks()
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.PublicKeyPem))
        {
            return NotFound();
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(_jwtOptions.PublicKeyPem);

        var parameters = rsa.ExportParameters(false);

        if (parameters.Modulus is null || parameters.Exponent is null)
        {
            return NotFound();
        }

        var keyId = string.IsNullOrWhiteSpace(_jwtOptions.KeyId)
            ? CreateKeyId(parameters.Modulus, parameters.Exponent)
            : _jwtOptions.KeyId;

        var response = new JwksResponse
        {
            Keys =
            [
                new JwkKeyResponse
                {
                    Kty = "RSA",
                    Use = "sig",
                    Alg = "RS256",
                    Kid = keyId,
                    N = Base64UrlEncode(parameters.Modulus),
                    E = Base64UrlEncode(parameters.Exponent)
                }
            ]
        };

        return Ok(response);
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string CreateKeyId(byte[] modulus, byte[] exponent)
    {
        var data = new byte[modulus.Length + exponent.Length];
        Buffer.BlockCopy(modulus, 0, data, 0, modulus.Length);
        Buffer.BlockCopy(exponent, 0, data, modulus.Length, exponent.Length);

        var hash = SHA256.HashData(data);
        return Base64UrlEncode(hash);
    }
}
