namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer.Contracts
{
    public interface INotificationServerHost
    {
        void Start(int port = 8081);
        void Stop();
        void Send(string channel, string message);
        void Broadcast(string message);
    }
}
