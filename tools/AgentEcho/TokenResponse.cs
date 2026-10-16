using System.Text.Json.Serialization;

namespace AgentEcho;

public sealed record TokenResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn
);