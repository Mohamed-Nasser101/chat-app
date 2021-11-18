using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        public static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                    OnlineUsers[username].Add(connectionId);
                else
                {
                    isOnline = true;
                    OnlineUsers.Add(username, new List<string> {connectionId});
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetConnectedUsers()
        {
            string[] users;
            lock (OnlineUsers)
            {
                users = OnlineUsers.OrderBy(u => u.Key)
                    .Select(u => u.Key).ToArray();
            }

            return Task.FromResult(users);
        }

        public Task<List<string>> GetUserConnection(string username)
        {
            List<string> connections;
            lock (OnlineUsers)
            {
                connections = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connections);
        }
    }
}