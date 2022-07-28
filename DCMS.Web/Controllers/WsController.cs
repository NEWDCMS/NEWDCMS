using DCMS.Core;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILogger = DCMS.Services.Logging.ILogger;
using DCMS.Services.Chat;
using Newtonsoft.Json;

namespace DCMS.Web.Controllers
{
    public partial class WsController : BasePublicController
    {
        private readonly ClientWebSocket webSocket = new ClientWebSocket();
        private readonly CancellationToken _cancellation = new CancellationToken();
        private IAccountingService _accountingService;

        public WsController(
           INotificationService notificationService,
           IStoreContext storeContext,
           ILogger loggerService,
           IAccountingService accountingService,
           IWorkContext workContext) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
        }

        /// <summary>
        /// https://www.jsdcms.com/ws/send
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult Send()
        {
            SendMsg();
            return Content("Succcess", "text/html");
        }


        ///客户端发送消息
        private async void SendMsg()
        {
            try
            {
                await webSocket.ConnectAsync(new Uri("ws://www.jsdcms.com?sid=" + 123), _cancellation);
                var msg = new MsgInfo { data = "客户端发送消息" };
                //发送的数据
                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
                var bsend = new ArraySegment<byte>(buffer);
                await webSocket.SendAsync(bsend, WebSocketMessageType.Binary, true, _cancellation);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "1", _cancellation);
                //记得一定要释放不然服务端还产生很多连接
                webSocket.Dispose();
            }
            catch (Exception)
            {
            }
        }

    }
}


