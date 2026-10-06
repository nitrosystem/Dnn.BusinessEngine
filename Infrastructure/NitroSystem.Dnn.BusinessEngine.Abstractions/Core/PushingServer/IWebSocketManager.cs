using System.Web.UI.WebControls.WebParts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer
{
    public interface IWebSocketManager
    {
        int? EnsureStarted();
        int? GetPort();
        bool IsRunning { get; }
    }
}
