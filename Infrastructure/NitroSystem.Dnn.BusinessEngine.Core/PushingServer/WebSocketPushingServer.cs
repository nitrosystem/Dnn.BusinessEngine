using System;
using WebSocketSharp.Server;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class WebSocketPushingServer
    {
        private readonly WebSocketServer _server;
        private readonly PushingServer _pushingServer;

        public WebSocketPushingServer(PushingServer pushingServer, int port = 8081)
        {
            _pushingServer = pushingServer;
            _server = new WebSocketServer($"ws://localhost:{port}");
            _server.AddWebSocketService<NotificationBehavior>("/notify");
        }

        public void Start()
        {
            _server.Start();
            Console.WriteLine("WebSocket PushingServer started.");
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}
