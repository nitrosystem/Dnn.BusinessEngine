using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NitroSystem.Dnn.BusinessEngine.Core.SseNotifier
{
    public class SseNotifier : ISseNotifier
    {
        public async Task Publish(string channel, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            await SseConnectionManager.PublishAsync(channel, json);
        }
    }
}
