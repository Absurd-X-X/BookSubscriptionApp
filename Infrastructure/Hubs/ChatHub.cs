using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    public class ChatHub : Hub
    {
        // Join a specific conversation room
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"conversation_{conversationId}");
        }
    }
}
