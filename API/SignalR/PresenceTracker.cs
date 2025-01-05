using System;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = [];
    
    public Task<bool> UserConnected(string userName, string connectionId)
    {
        var isOnline = false;
        lock (OnlineUsers) 
        {
            if (OnlineUsers.ContainsKey(userName))
            {
                OnlineUsers[userName].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(userName, [connectionId]);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string userName, string connectionId) 
    {
        var isOffline = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(userName)) return Task.FromResult(isOffline);  //It is false here because nothing hss changed, we don't have to notify other users

            OnlineUsers[userName].Remove(connectionId);

            if (OnlineUsers[userName].Count == 0)
            {
                OnlineUsers.Remove(userName);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string userName)
    {
        List<string> connectionIds;

        if (OnlineUsers.TryGetValue(userName, out var connections))
        {
            lock (connections)
            {
                connectionIds = connections.ToList();
            }
        }
        else 
        {
            connectionIds = [];
        }

        return Task.FromResult(connectionIds);
    }
}
