using Microsoft.AspNetCore.SignalR;

namespace ProtoEcho.Server.Hubs;

public sealed class EchoHub : Hub
{
    public async Task Echo(string message)
    {
        await Clients.Caller.SendAsync("AgentEcho", message);
    }
}