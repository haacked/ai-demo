using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class ChatHub : Hub
{
    /// <summary>
    /// When a new message is received, broadcast it to all clients.
    /// </summary>
    /// <param name="username">The name of the person sending the message.</param>
    /// <param name="message">The message text.</param>
    public async Task NewMessage(string username, string message) =>
        await Clients.All.SendAsync("messageReceived", username, message);
}