using Microsoft.AspNetCore.SignalR;

namespace VersionControl.Api.Hubs;

public class MonitoringHub : Hub
{
    public async Task SendViolation(object violation)
    {
        await Clients.All.SendAsync(
            "ViolationReceived",
            violation);
    }

    public async Task SendComputerStatus(object computer)
    {
        await Clients.All.SendAsync(
            "ComputerStatusChanged",
            computer);
    }

    public async Task SendPolicyUpdate()
    {
        await Clients.All.SendAsync(
            "PoliciesUpdated");
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine(
            $"Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        Console.WriteLine(
            $"Client disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }
}