namespace AgentEcho;

public sealed class KeycloakTokenProvider(HttpClient httpClient, string tokenEndpoint, string clientId, string clientSecret)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    
    private string? _accessToken;
    private DateTimeOffset _expiresAt;
    
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_accessToken is null || DateTimeOffset.UtcNow >= _expiresAt.AddSeconds(-30))
            {
                await RequestToken(cancellationToken);
            }
            
            return _accessToken!;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task RequestToken(CancellationToken cancellationToken)
    {
        Console.WriteLine("Requesting new access token from Keycloak...");
        
        using var response = await httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            }),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            throw new InvalidOperationException("Failed to retrieve access token from Keycloak");
        
        _accessToken = token.AccessToken;
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
    }
}