using Microsoft.AspNetCore.SignalR;

namespace ProtoEcho.Server.Hubs;

public sealed class EchoHub : Hub
{
    public async Task Echo(string message)
    {
        await Clients.Caller.SendAsync("AgentEcho", message);
    }
    
    public string WhoAmI()
    {
        return $"Authenticated: {Context.User?.Identity?.IsAuthenticated}, " +
               $"Client: {Context.User?.FindFirst("azp")?.Value}";
    }
}