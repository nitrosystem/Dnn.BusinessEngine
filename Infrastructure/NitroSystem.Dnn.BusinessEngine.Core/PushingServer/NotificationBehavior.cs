using WebSocketSharp;
using WebSocketSharp.Server;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationBehavior : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            // فقط echo برای تست
            Send($"Server received: {e.Data}");
        }
    }
}
