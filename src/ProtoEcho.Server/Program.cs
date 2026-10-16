using ProtoEcho.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => "ProtoEcho is running");

app.MapHub<EchoHub>("/hubs/echo");

app.Run();
