using System;
using System.Linq;
using WebSocketSharp.Server;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationServerHost : INotificationServerHost
    {
        private WebSocketServer _wsServer;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);
        private System.Timers.Timer _watchdog;

        public int Start()
        {
            try
            {
                var path = "/notify";
                var port = GetFreePort();

                _wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");
                _wsServer.AddWebSocketService<NotificationServer>(path);
                _wsServer.Start();

                SetupWatchdog();

                return port;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public void Stop()
        {
            if (_wsServer != null && _wsServer.IsListening)
                _wsServer.Stop();
            
            _watchdog?.Stop();
            _watchdog?.Dispose();
        }

        private void SetupWatchdog()
        {
            _watchdog = new System.Timers.Timer(10000);
            _watchdog.Elapsed += (_, __) => CleanupInactiveClients();
            _watchdog.Start();
        }

        private void CleanupInactiveClients()
        {
            var now = DateTime.UtcNow;
            var inactive = NotificationServer.LastSeen
                .Where(x => now - x.Value > _timeout)
                .Select(x => x.Key)
                .ToList();

            foreach (var client in inactive)
            {
                try { client.Close(); } catch { }
            }
        }

        private int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();

            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            
            return port;
        }
    }
}
