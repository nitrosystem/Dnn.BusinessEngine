using System;
using System.Net;
using WebSocketSharp.Server;
using NitroSystem.Dnn.BusinessEngine.Core.PushingServer.Contracts;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationServerHost : INotificationServerHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly object _lock = new object();
        private WebSocketServer _server;
        private bool _isStarted;

        public NotificationServerHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start(int port = 8081)
        {
            lock (_lock)
            {
                if (_isStarted && _server?.IsListening == true)
                {
                    Console.WriteLine($"[NotificationServerHost] ⚠️ Server is already running on port {port}. Skipping start.");
                    return;
                }

                try
                {
                    _server = new WebSocketServer(IPAddress.Any, port);
                    _server.AddWebSocketService<NotificationServer>("/notify");
                    _server.Start();

                    _isStarted = true;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    Console.WriteLine($"[NotificationServerHost] ⚠️ Port {port} is already in use. The server will not start again.");
                    return; // یا log کن و رد شو
                }

                Console.WriteLine($"[NotificationServerHost] ✅ WebSocket server started on port {port}.");
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_server != null && _server.IsListening)
                {
                    _server.Stop();
                    _isStarted = false;
                    Console.WriteLine("[NotificationServerHost] ⏹️ WebSocket server stopped.");
                }
                else
                {
                    Console.WriteLine("[NotificationServerHost] ⚠️ Stop called but server was not running.");
                }
            }
        }

        public void Send(string channel, string message)
        {
            var _notificationServer = _serviceProvider.GetRequiredService<NotificationServer>();
            _notificationServer.SendToChannel(channel, message);
        }

        public void Broadcast(string message)
        {
            var _notificationServer = _serviceProvider.GetRequiredService<NotificationServer>();
            _notificationServer.Broadcast(message);
        }
    }
}
