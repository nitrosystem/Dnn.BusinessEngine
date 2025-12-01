using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationServerProxy : INotificationServer
    {
        public void SendToChannel(string channel, object payload)
        {
            NotificationServer.SendToChannel(channel, payload);
        }

        public void Broadcast(object payload)
        {
            NotificationServer.SendToChannel("broadcast", payload);
        }
    }

}
