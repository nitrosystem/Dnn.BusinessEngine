using WebSocketSharp.Server;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class WebSocketManager : IWebSocketManager
    {
        private readonly INotificationServerHost _notificationServerHost;
        private static object _lock = new object();
        private static bool _started = false;
        private static int? _port;

        public WebSocketManager(INotificationServerHost notificationServerHost)
        {
            _notificationServerHost = notificationServerHost;
        }

        public bool IsRunning => _started;

        public int? EnsureStarted()
        {
            if (_started) return null;

            lock (_lock)
            {
                if (_started) return null;

                _port = _notificationServerHost.Start();
                if (_port != -1)
                    _started = true;

                return _port;
            }
        }

        public int? GetPort() => _port;
    }

}
