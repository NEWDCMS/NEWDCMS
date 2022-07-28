using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Services.Chat
{
    /// <summary>
    /// WebSocket中间件=
    /// </summary>
    public class WebSocketMiddleware
    {
        private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        private readonly RequestDelegate _next;

        /// <summary>
        /// WebSocket中间件
        /// </summary>
        /// <param name="next"></param>
        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            CancellationToken ct = context.RequestAborted;
            var currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            string socketId_Expand = Guid.NewGuid().ToString(); 
            string socketId = context.Request.Query["sid"].ToString() + socketId_Expand;
            if (!_sockets.ContainsKey(socketId))
            {
                _sockets.TryAdd(socketId, currentSocket);
            }
            //_sockets.TryRemove(socketId, out dummy);
            //_sockets.TryAdd(socketId, currentSocket);

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                string response = await ReceiveStringAsync(currentSocket, ct);
                //WriteInfo("WebSocket发送消息：" + response);
                MsgInfo msg = JsonConvert.DeserializeObject<MsgInfo>(response);

                if (string.IsNullOrEmpty(response))
                {
                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    continue;
                }

                foreach (var socket in _sockets)
                {
                    if (socket.Value.State != WebSocketState.Open)
                    {
                        continue;
                    }
                    //控制只有接收者才能收到消息，并且用户所有的客户端都可以同步收到
                    if (socket.Key.Contains(msg.to_id))
                    {
                        await SendStringAsync(socket.Value, JsonConvert.SerializeObject(msg), ct);
                    }
                }
            }

            //_sockets.TryRemove(socketId, out dummy);

            await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            currentSocket.Dispose();
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            //当接收到文本消息时，对当前服务器上所有web socket连接进行广播
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}