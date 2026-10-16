using Microsoft.AspNetCore.SignalR.Client;

await using var connection = new HubConnectionBuilder()
	.WithUrl("http://localhost:5000/hubs/echo")
	.Build();

connection.On<string>("AgentEcho", m => { Console.WriteLine($"Recieved: {m}"); });

await connection.StartAsync();

Console.WriteLine("Connected.");

await connection.SendAsync("Echo", "Hello ProtoEcho!");

Console.WriteLine("Echo sent.");

Console.ReadLine();
