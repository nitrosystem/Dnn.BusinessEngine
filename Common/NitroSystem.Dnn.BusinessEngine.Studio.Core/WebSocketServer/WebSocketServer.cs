using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer
{
    public class WebSocketServer : IDisposable
    {
        private ClientWebSocket _ws;
        private readonly Uri _serverUri = new Uri("ws://dnndev.new/WebSocketHandler.ashx");

        public WebSocketServer()
        {
            try
            {
                _ws = new ClientWebSocket();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (_ws.State == WebSocketState.Open)
                    return;

                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(_serverUri, CancellationToken.None);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task SendMessageToClientAsync(string status,int progress )
        {
            await SendMessageToClientAsync(progress, status);
        }
        public async Task SendMessageToClientAsync(int progress, string status)
        {
            try
            {
                var message = JsonConvert.SerializeObject(new { progress, status });

                if (_ws.State != WebSocketState.Open)
                    await ConnectAsync(); // اتصال مجدد در صورت قطع شدن

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                var bytesToSend = new ArraySegment<byte>(messageBytes);
                await _ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task CloseAsync()
        {
            try
            {
                if (_ws.State == WebSocketState.Open)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }

                _ws.Dispose();
            }
            catch (Exception ex)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                _ws?.Dispose();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
