using DCMS.Core;
using DCMS.Core.Domain.Chat;
using DCMS.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于RTC服务
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/rtc")]
    public class RTCController : BaseAPIController
    {
        private readonly IChatUserService _userService;
        private readonly IChatService   _chatService;

        private readonly IConnectionService _connectionService;
        private readonly IHubContext<ChatHub> _qrHub;


        public RTCController(ILogger<BaseAPIController> logger,
            IChatUserService userService,
            IConnectionService connectionService,
            IHubContext<ChatHub> qrHub,
            IChatService chatService) : base(logger)
        {
            _userService = userService;
            _chatService = chatService;
            _connectionService = connectionService;
            _qrHub = qrHub;

        }

        [AllowAnonymous]
        [HttpGet("heartbeat")]
        public async Task<APIResult<object>> SendHeartbeat(string msg)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var connectionIds = _connectionService.GetConnectedIds();
                    if(connectionIds!=null&& connectionIds.Any())
                    {
                        foreach(var c in connectionIds)
                        {
                            await _qrHub.Clients.Client(c).SendAsync("OnHeartbeat", msg);
                        }
                    }
    
                    return this.Successful("广播成功");

                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }


        [AllowAnonymous]
        [HttpPost("send/message")]
        public async Task<APIResult<object>> SendMessage(int userId, string msg, string call = "Receive")
        {
            try
            {
                if (userId == 0 || string.IsNullOrEmpty(msg))
                {
                    return this.Error("参数错误");
                }

                var connectionId = _connectionService.GetConnectionIdByUser(userId);
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _qrHub.Clients.Client(connectionId)
                               .SendAsync(call, msg);

                    return this.Successful("发送成功");
                }
                else
                {
                    return this.Error("参数错误");
                }
            }
            catch (Exception ex)
            {
                return this.Error(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("online")]
        public async Task<APIResult<IList<User>>> GetOnlineUsers(int? storeId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var users = _userService.GetUsers(storeId);
                    return this.Successful2("获取成功", users.ToList());
                }
                catch (Exception ex)
                {
                    return this.Error3<IList<User>>(ex.Message);
                }
            });
        }


        [AllowAnonymous]
        [HttpGet("utcs")]
        public async Task<ConcurrentDictionary<string, string>> GetUTCS()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var usts = _userService.UTCS();
                    return usts;
                }
                catch (Exception)
                {
                    return new ConcurrentDictionary<string, string>();
                }
            });
        }


        [AllowAnonymous]
        [HttpGet("ctus")]
        public async Task<ConcurrentDictionary<string, string>> GetCTUS()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var ctus = _userService.CTUS();
                    return ctus;
                }
                catch (Exception)
                {
                    return new ConcurrentDictionary<string, string>();
                }
            });
        }

    }
}
