using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase
{
    public interface IEngineNotifier
    {
        Task NotifyProgress(string message, double? percent = null);
        void PushingNotification(string channel, object data);
    }
}
