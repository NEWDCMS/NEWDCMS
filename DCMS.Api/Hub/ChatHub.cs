using DCMS.Core.Domain.Chat;
using DCMS.Services.Chat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DCMS.Api
{
    public class ChatHub : Hub
    {
        private readonly IChatUserService _userService;
        private readonly IChatService _chatService;
        private readonly IConnectionService _connectionService;

        public ChatHub(IChatUserService userService, 
            IChatService chatService, 
            IConnectionService connectionService)
        {
            this._userService = userService;
            this._chatService = chatService;
            this._connectionService = connectionService;
        }

        public async Task<User> Login(User data)
        {
            var UUID = Guid.NewGuid().ToString();

            var user = _userService.LoginUser(data.StoreId,
                data.UserId,
                Context.ConnectionId,
                data.Name,
                data.Avatar,
                data.MobileNumber,
                UUID);

            //获取除触发当前调用的连接之外的所有连接的调用方
            if (user != null)
                await Clients.Others
                    .SendAsync("UserConnected", user);

            return user;
        }

        public async Task LogOut(User user)
        {
            _userService.LogOut(user);

            await Clients.Others
                .SendAsync("UserDisconnected", user);
        }


        public void CheckLogin(string connectionId)
        {
            _connectionService.ReplaceUserConnectionIdWithAnother(connectionId, Context.ConnectionId);
        }

        public IList<User> GetConnectedUsers(User user)
        {
            var users = _userService.GetConnectedUsersExceptMe(user).ToList();
            if (users != null && users.Any())
                return users?.Where(u => u.Name != user.Name)?.ToList();
            else
                return new List<User>();
        }


        public IEnumerable<Message> GetMessagesByUsers(int sender, int reciever, int limit = 100)
        {
            var messages = _chatService.GetMessagesByUsers(sender, reciever, limit);

            if (messages == null) 
                return null;

            return messages;
        }


        /// <summary>
        /// 发送消息给指定接收用户
        /// </summary>
        /// <param name="message"></param>
        /// <param name="storeId"></param>
        /// <param name="recieverName"></param>
        /// <param name="recieverId"></param>
        public async Task SendMessageToUser(Message message, int storeId, string recieverName, int recieverId)
        {
            await Clients.Client(_connectionService.GetConnectionIdByUser(recieverId))
                .SendAsync("ReceiveMessage", message);

            _chatService.AddMessageToChatByNames(message, storeId, recieverName, recieverId);
        }

        /// <summary>
        /// 重写连接事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller
                .SendAsync("MakeLogin", Context.ConnectionId);

            await base.OnConnectedAsync();
        }


        /// <summary>
        /// 重写连接断开事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //查询用户
            string disconnectedUser = _connectionService.GetUserNameByConnectionId(Context.ConnectionId);
            //如果用户存在，则注销用户
            if (disconnectedUser != null)
            {
                var u = _connectionService.GetUserNameByConnectionId(Context.ConnectionId);
                int.TryParse(u, out int userId);
                var user = _userService.GetUserById(userId);
                if (user != null)
                {
                    await LogOut(user);
                }
                else
                {
                    await LogOut(new User
                    {
                        UserId = userId
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }


    /*
    public class ChatHub : Hub
    {
        private static readonly object SyncObj = new object();
        private readonly IStaticCacheManager _cacheManager;

        /// <summary>
        /// 构造时对Program.MyHub赋值
        /// </summary>
        public ChatHub(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// 实现推送扫码成功的用户信息的方法
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="uuid"></param>
        /// <param name="userId"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public async Task SendUserInfo(string connectionId, string uuid, int userId, string pwd)
        {
            //调用客户端的 GetUserInfo 方法 返回用户信息
            await Clients.Client(connectionId)
                .SendAsync("GetUserInfo", uuid, userId, pwd);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMessage(string msg)
        {
            var client = Program.ClientInfoList?.Where(u => u.ConnectionId == Context.ConnectionId)
                .SingleOrDefault();

            var chatName = $"DCMS_{client.StoreId}";

            if (client == null)
            {
                await Clients.Client(Context.ConnectionId)
                    .SendAsync("System", "您已不在聊天室,请重新加入");
            }
            else
            {
                await Clients.GroupExcept(chatName, new[] { Context.ConnectionId })
                    .SendAsync("Receive", new
                    {
                        Msg = msg,
                        Name = client.Name,
                        Avatar = client.Avatar
                    });
            }
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendHeartbeat(string msg)
        {
            try
            {
                //所有客户端发送
                foreach (var c in Program.ClientInfoList)
                {
                    await Clients.Client(c.ConnectionId).SendAsync("OnHeartbeat", msg);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 实现注册方法
        /// </summary>
        public async Task Register()
        {
            //生成UUID
            var UUID = Guid.NewGuid().ToString();

            //string key = $"QRCLIENTINFOLIST_UUID_{UUID}";
            //var cacheKey = new CacheKey(key) { CacheTime = 5 };
            //var uuid = _cacheManager.Get<string>(cacheKey, null) ?? new List<ClientInfo>();

            var http = Context.GetHttpContext();
            var client = Program.ClientInfoList?.Where(u => u.ConnectionId == Context.ConnectionId).SingleOrDefault();

            int.TryParse(http?.Request?.Query["userId"] ?? "0", out int userid);
            int.TryParse(http?.Request?.Query["storeId"] ?? "0", out int storeId);
            var chatName = $"DCMS_{storeId}";

            if (client == null)
            {
                try
                {
         
                    client = new ClientInfo()
                    {
                        UserId = userid,
                        StoreId = storeId,
                        Name = http.Request.Query["name"],
                        Avatar = http.Request.Query["avatar"],
                        ConnectionId = Context.ConnectionId,
                        UUID = UUID
                    };
                }
                catch(Exception ex)
                { }

                if (client != null)
                    Program.ClientInfoList.Add(client);
            }
            else
            {
                client.UUID = UUID;
            }


            //添加组
            await Groups.AddToGroupAsync(Context.ConnectionId, chatName);

            //发送消息
            await Clients.GroupExcept(chatName, new[] { Context.ConnectionId })
                .SendAsync("System", $"用户{client.Name}加入了群");

            //向客户端发送消息
            await Clients.Client(Context.ConnectionId).SendAsync("System", JsonConvert.SerializeObject(new
            {
                Susccess = true,
                Msg = $"成功加入{chatName}",
                UUID
            }));

            //存储UUID
            //_cacheManager.Set(cacheKey, UUID);
        }


        /// <summary>
        /// 重写连接事件 目前没实现功能,你可以在这记日志或者干点别的事情
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 重写连接断开事件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);

            bool isRemoved = false;
            //查询用户
            var client = Program.ClientInfoList.Where(u => u.ConnectionId == Context.ConnectionId)
                .SingleOrDefault();

            var chatName = $"DCMS_{client.StoreId}";

            lock (SyncObj)
            {
                //判断用户是否存在，存在则删除
                if (client != null)
                {
                    isRemoved = Program.ClientInfoList.Remove(client);
                }
            }

            //移除组
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatName);

            if (isRemoved)
            {
                await Clients.GroupExcept(chatName, new[] { Context.ConnectionId })
                    .SendAsync("System", $"用户{client.Name}退出了群聊");
            }
        }
    }

    */
}
