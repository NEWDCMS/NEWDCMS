using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DCMS.Services.Chat
{
    public class ConnectionService : IConnectionService
    {
        /// <summary>
        /// 用户转连接 key:UserId，value:ConnectionId
        /// </summary>
        public ConcurrentDictionary<string, string> UserToConnectionId { get; set; }

        /// <summary>
        /// 连接转用户 key:ConnectionId，value:UserId
        /// </summary>
        public ConcurrentDictionary<string, string> ConnectionIdToUser { get; set; }


        public ConnectionService()
        {
            UserToConnectionId = new ConcurrentDictionary<string, string>();
            ConnectionIdToUser = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// 移除用户
        /// </summary>
        /// <param name="connectionId"></param>
        public void RemoveUserFromConnectedUsersByConnectionId(string connectionId)
        {
            string ov = "";
            string c = ConnectionIdToUser.FirstOrDefault(u => u.Key == connectionId).Value;

            if (!string.IsNullOrEmpty(c))
            {
                UserToConnectionId.TryRemove(c, out ov);
                ConnectionIdToUser.TryRemove(connectionId, out ov);
            }
        }


        /// <summary>
        /// 移除用户
        /// </summary>
        /// <param name="name"></param>
        public void RemoveUserFromConnectedUsersById(int userId)
        {
            string ov = "";
            string u = UserToConnectionId.FirstOrDefault(u => u.Key == userId.ToString()).Value;
            if (!string.IsNullOrEmpty(u))
            {
                ConnectionIdToUser.TryRemove(u, out ov);
                UserToConnectionId.TryRemove(userId.ToString(), out ov);
            }
        }


        /// <summary>
        /// 替换连接
        /// </summary>
        /// <param name="oldConnectionId"></param>
        /// <param name="newConnectionId"></param>
        public void ReplaceUserConnectionIdWithAnother(string oldConnectionId, string newConnectionId)
        {
            KeyValuePair<string, string> tempConId = ConnectionIdToUser.FirstOrDefault(c => c.Key == oldConnectionId);

            if (string.IsNullOrEmpty(tempConId.Key)) return;

            RemoveUserFromConnectedUsersByConnectionId(oldConnectionId);

            AddUserToConnectedUsers(tempConId.Value, newConnectionId);

            Debug.WriteLine($"{tempConId.Value} Connected Again. New connectionId : {newConnectionId}");
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        public void AddUserToConnectedUsers(string userId, string connectionId)
        {
            //连接不存在时
            if (!ConnectionIdToUser.ContainsKey(connectionId))
            {
               //用户是否存在
                var user = ConnectionIdToUser.Where(s => s.Value == userId).FirstOrDefault();
                //如果用户的连接存在时且不等于新的连接时踢出早前连接
                if (user.Key != connectionId)
                {
                    //移除早前连接（踢出早前）
                    RemoveUserFromConnectedUsersByConnectionId(user.Key);
                }

                //重新添加
                ConnectionIdToUser.AddOrUpdate(connectionId, userId.ToString(), (key, old) =>
                {
                    return old;
                });
            }

            if (!UserToConnectionId.ContainsKey(userId))
            {
                UserToConnectionId.AddOrUpdate(userId, connectionId, (key, old) =>
                {
                    return old;
                });
            }

        }

        public List<string> GetConnectedUsers() => ConnectionIdToUser.Values.ToList();
        public List<string> GetConnectedIds() => UserToConnectionId.Values.ToList();


        public string GetConnectionIdByUser(int recieverId) => UserToConnectionId[recieverId.ToString()];
        public string GetUserNameByConnectionId(string connectionId)
        {
            return ConnectionIdToUser.GetValueOrDefault(connectionId);
        }

        /// <summary>
        /// 判断是否登录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserAlreadyLoggedIn(int userId)
        {
            return UserToConnectionId.Any(u => u.Key == userId.ToString());
        }
    }
}
