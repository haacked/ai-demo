using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class ChatHub : Hub
{
    public async Task NewMessage(string username, string message) =>
        await Clients.All.SendAsync("messageReceived", username, message);
}