using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

var configuration = new ConfigurationBuilder()
	.AddUserSecrets<Program>()
	.Build();

var clientId = configuration["Keycloak:ClientId"] 
               ?? throw new InvalidOperationException("ClientId not found in secrets.");

var clientSecret = configuration["Keycloak:ClientSecret"] 
               ?? throw new InvalidOperationException("ClientSecret not found in secrets.");

using var httpClient = new HttpClient();

const string tokenEndpoint = "http://localhost:8180/realms/protoecho/protocol/openid-connect/token";

using var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent([
	new KeyValuePair<string, string>("grant_type", "client_credentials"),
	new KeyValuePair<string, string>("client_id", clientId),
	new KeyValuePair<string, string>("client_secret", clientSecret)
]));

response.EnsureSuccessStatusCode();

var token = await response.Content.ReadFromJsonAsync<TokenResponse>()
	?? throw new InvalidOperationException("Empty token response.");

Console.WriteLine($"Access token obtained. Expires in {token.ExpiresIn} seconds.	");

await using var connection = new HubConnectionBuilder()
	.WithUrl("http://localhost:5000/hubs/echo", options =>
	{
		options.Transports = HttpTransportType.WebSockets;
		options.AccessTokenProvider = () => Task.FromResult<string?>(token.AccessToken);
	})
	.Build();

connection.On<string>("AgentEcho", m => { Console.WriteLine($"Recieved: {m}"); });

await connection.StartAsync();

Console.WriteLine("Connected.");

await connection.SendAsync("Echo", "Hello ProtoEcho!");

Console.WriteLine("Echo sent.");

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

var sequence = 0;

while (await timer.WaitForNextTickAsync())
{
	++sequence;
	
	if (sequence == 36)
	{
		Console.WriteLine("Disconnecting...");

		await connection.StopAsync();

		Console.WriteLine("Reconnecting with original token...");

		try
		{
			await connection.StartAsync();

			Console.WriteLine("Reconnected.");
			
			var identity = await connection.InvokeAsync<string>("WhoAmI");

			Console.WriteLine(identity);
		}
		catch (Exception ex)
		{
			Console.WriteLine(
				$"Reconnection failed: {ex.GetType().Name}: {ex.Message}");

			break;
		}
	}
	
	try
	{
		await connection.SendAsync("Echo", $"Message {sequence} as {DateTimeOffset.Now:HH:mm:ss}");
		
		Console.WriteLine($"Send message {sequence}");
	}
	catch (Exception e)
	{
		Console.WriteLine($"Send failed: {e.GetType().Name}: {e.Message}");
		break;
	}
}

//Console.ReadLine();

public sealed record TokenResponse(
	[property: JsonPropertyName("access_token")]
	string AccessToken,
	[property: JsonPropertyName("expires_in")]
	int ExpiresIn
);
