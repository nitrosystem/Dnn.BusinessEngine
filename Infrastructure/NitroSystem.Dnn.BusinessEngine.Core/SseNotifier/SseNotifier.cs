using Newtonsoft.Json;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.SseNotifier
{
    public class SseNotifier : ISseNotifier
    {
        public async Task Publish(string scenario, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            await SseConnectionManager.PublishAsync(scenario, json);
        }
    }
}
