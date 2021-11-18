using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            //await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            await _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            var currentUsers = await _presenceTracker.GetConnectedUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            await _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            
            // var currentUsers = await _presenceTracker.GetConnectedUsers();
            // await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}