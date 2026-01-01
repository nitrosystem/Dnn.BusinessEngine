using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.SseNotifier
{
    public interface ISseNotifier
    {
        Task Publish(string scenario, object payload);
    }
}
