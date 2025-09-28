using Microsoft.AspNetCore.SignalR;
using Obeli_K.Enums;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using System.Security.Claims;

namespace Obeli_K.Hubs
{
    public class NotificationsHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendNotification(string userId, string message, TypeNotification type = TypeNotification.Info)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Type = type.ToString(),
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendNotificationToGroup(string groupName, string message, TypeNotification type = TypeNotification.Info)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Type = type.ToString(),
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendNotificationToRole(RoleType role, string message, TypeNotification type = TypeNotification.Info)
        {
            var groupName = $"Role_{role}";
            await Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Type = type.ToString(),
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendNotificationToAll(string message, TypeNotification type = TypeNotification.Info)
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                Message = message,
                Type = type.ToString(),
                Timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnConnectedAsync()
        {
            // Ajouter l'utilisateur au groupe de son rôle
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                
                foreach (var role in roles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, role);
                }
                
                // Ajouter spécifiquement au groupe des prestataires si applicable
                if (roles.Contains("PrestataireCantine"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "PrestataireCantine");
                }
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
