using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;

var configuration = new ConfigurationBuilder()
	.AddUserSecrets<Program>()
	.Build();

var clientId = configuration["Keycloak:ClientId"] 
               ?? throw new InvalidOperationException("ClientId not found in secrets.");

var clientSecret = configuration["Keycloak:ClientSecret"] 
               ?? throw new InvalidOperationException("ClientSecret not found in secrets.");

using var httpClient = new HttpClient();

var tokenEndpoint = "http://localhost:8180/realms/protoecho/protocol/openid-connect/token";

using var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new[]
{
	new KeyValuePair<string, string>("grant_type", "client_credentials"),
	new KeyValuePair<string, string>("client_id", clientId),
	new KeyValuePair<string, string>("client_secret", clientSecret)
}));

response.EnsureSuccessStatusCode();

var token = await response.Content.ReadFromJsonAsync<TokenResponse>()
	?? throw new InvalidOperationException("Empty token response.");

Console.WriteLine($"Access token obtained. Expires in {token.ExpiresIn} seconds.	");

await using var connection = new HubConnectionBuilder()
	.WithUrl("http://localhost:5000/hubs/echo", options =>
	{
		options.AccessTokenProvider = () => Task.FromResult<string?>(token.AccessToken);
	})
	.Build();

connection.On<string>("AgentEcho", m => { Console.WriteLine($"Recieved: {m}"); });

await connection.StartAsync();

Console.WriteLine("Connected.");

await connection.SendAsync("Echo", "Hello ProtoEcho!");

Console.WriteLine("Echo sent.");

Console.ReadLine();

public sealed record TokenResponse(
	[property: JsonPropertyName("access_token")]
	string AccessToken,
	[property: JsonPropertyName("expires_in")]
	int ExpiresIn
);