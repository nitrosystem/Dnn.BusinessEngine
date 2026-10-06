using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.SseNotifier
{
    public static class SseConnectionManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<SseClient>> _clients = new();

        public static void AddClient(SseClient client)
        {
            var bag = _clients.GetOrAdd(
                client.Channel,
                _ => new ConcurrentBag<SseClient>()
            );

            bag.Add(client);
        }

        public static async Task PublishAsync(string channel, string data)
        {
            if (_clients.TryGetValue(channel, out var bag))
            {
                foreach (var client in bag)
                {
                    try
                    {
                        await client.Writer.WriteAsync($"data: {data}\n\n");
                        await client.Writer.FlushAsync();
                    }
                    catch
                    {
                        // client disconnected – ignore
                    }
                }
            }
        }

        public static void RemoveClient(string channel)
        {
            _clients.TryRemove(channel, out _);
        }
    }
}
