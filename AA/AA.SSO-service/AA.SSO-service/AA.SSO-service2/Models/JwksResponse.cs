using System.Text.Json.Serialization;

namespace AA.SSO_service2.Models;

public class JwksResponse
{
    [JsonPropertyName("keys")]
    public List<JwkKeyResponse> Keys { get; set; } = [];

}