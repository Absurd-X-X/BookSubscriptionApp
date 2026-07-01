using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        // Each user joins their own private group
        // so notifications are only sent to them
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"user_{userId}");
        }
    }
}
