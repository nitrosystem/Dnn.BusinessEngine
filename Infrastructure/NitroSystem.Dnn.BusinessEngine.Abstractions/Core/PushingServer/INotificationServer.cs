namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer
{
    public interface INotificationServer
    {
        void SendToChannel(string channel, object payload);
        void Broadcast(object payload);
    }
}
