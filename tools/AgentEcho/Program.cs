using AgentEcho;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

const string tokenEndpoint = "http://localhost:8180/realms/protoecho/protocol/openid-connect/token";
const string echoHub = "http://localhost:5000/hubs/echo";

var configuration = new ConfigurationBuilder()
	.AddUserSecrets<Program>()
	.Build();

var clientId = configuration["Keycloak:ClientId"] 
               ?? throw new InvalidOperationException("ClientId not found in secrets.");

var clientSecret = configuration["Keycloak:ClientSecret"] 
               ?? throw new InvalidOperationException("ClientSecret not found in secrets.");

using var httpClient = new HttpClient();

var tokenProvider = new KeycloakTokenProvider(
	httpClient,
	tokenEndpoint, 
	clientId,
	clientSecret);

await using var connection = new HubConnectionBuilder()
	.WithUrl(echoHub, options =>
	{
		options.Transports = HttpTransportType.WebSockets;
		options.AccessTokenProvider = () => tokenProvider.GetAccessTokenAsync();
	})
	.Build();

connection.On<string>("AgentEcho", m => { Console.WriteLine($"Received: {m}"); });

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

		Console.WriteLine("Reconnecting with token provider...");

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

