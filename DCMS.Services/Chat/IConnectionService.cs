using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DCMS.Services.Chat
{
    public interface IConnectionService
    {
        ConcurrentDictionary<string, string> ConnectionIdToUser { get; set; }
        ConcurrentDictionary<string, string> UserToConnectionId { get; set; }

        void AddUserToConnectedUsers(string userId, string connectionId);
        List<string> GetConnectedIds();
        List<string> GetConnectedUsers();
        string GetConnectionIdByUser(int recieverId);
        string GetUserNameByConnectionId(string connectionId);
        bool IsUserAlreadyLoggedIn(int userId);
        void RemoveUserFromConnectedUsersByConnectionId(string connectionId);
        void RemoveUserFromConnectedUsersById(int userId);
        void ReplaceUserConnectionIdWithAnother(string oldConnectionId, string newConnectionId);
    }
}