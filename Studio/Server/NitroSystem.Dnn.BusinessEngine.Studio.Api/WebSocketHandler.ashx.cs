using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.WebSockets;
using NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    public class WebSocketHandler : IHttpHandler
    {
        private static readonly IList<WebSocket> Clients = new List<WebSocket>();
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(ProcessSocketRequest);
        }

        private async Task ProcessSocketRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;

            // add socket to socket list
            Locker.EnterWriteLock();
            try
            {
                Clients.Add(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            // maintain socket
            while (true)
            {
                var buffer = new byte[1024];

                // async wait for a change in the socket
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (socket.State == WebSocketState.Open)
                {
                    var receivedData = new byte[result.Count];
                    Array.Copy(buffer, receivedData, result.Count);

                    // echo to all clients
                    foreach (var client in Clients)
                    {
                        await client.SendAsync(new ArraySegment<byte>(receivedData), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    // client is no longer available - delete from list
                    Locker.EnterWriteLock();
                    try
                    {
                        Clients.Remove(socket);
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }

                    break;
                }
            }
        }

        public bool IsReusable { get { return false; } }
    }
}