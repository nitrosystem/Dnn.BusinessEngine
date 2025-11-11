using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationServer : WebSocketBehavior
    {
        // هر کانال می‌تونه چند کلاینت داشته باشه
        private static readonly ConcurrentDictionary<string, ConcurrentBag<WebSocket>> _channels =
            new ConcurrentDictionary<string, ConcurrentBag<WebSocket>>();

        // شناسه‌ی کانال کلاینت فعلی
        private string _channelName;

        protected override void OnOpen()
        {
            // کانال را از QueryString می‌گیریم
            var query = Context.QueryString["channel"];
            _channelName = string.IsNullOrEmpty(query) ? "default" : query;

            var clients = _channels.GetOrAdd(_channelName, _ => new ConcurrentBag<WebSocket>());
            clients.Add(Context.WebSocket);

            Console.WriteLine($"[NotificationServer] Client connected to channel '{_channelName}'.");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (!string.IsNullOrEmpty(_channelName) && _channels.TryGetValue(_channelName, out var clients))
            {
                var updated = new ConcurrentBag<WebSocket>(clients.Where(c => c != Context.WebSocket));
                _channels[_channelName] = updated;
                Console.WriteLine($"[NotificationServer] Client disconnected from '{_channelName}'.");
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine($"[NotificationServer] Message from '{_channelName}': {e.Data}");
        }

        public void SendToChannel(string channel, object data)
        {
            SendToChannel(channel, JsonConvert.SerializeObject(data));
        }

        // ارسال پیام به یک کانال خاص
        public void SendToChannel(string channel, string message)
        {
            if (_channels.TryGetValue(channel, out var clients))
            {
                var payload = JsonConvert.SerializeObject(new
                {
                    Channel = channel,
                    Message = message
                });

                foreach (var client in clients)
                {
                    if (client.ReadyState == WebSocketState.Open)
                        client.Send(payload);
                }

                Console.WriteLine($"[NotificationServer] Message sent to channel '{channel}': {message}");
            }
            else
            {
                Console.WriteLine($"[NotificationServer] Channel '{channel}' not found.");
            }
        }

        // ارسال پیام به همه‌ی کلاینت‌ها در همه‌ی کانال‌ها
        public void Broadcast(string message)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                Channel = "*",
                Message = message
            });

            foreach (var kvp in _channels)
            {
                foreach (var client in kvp.Value)
                {
                    if (client.ReadyState == WebSocketState.Open)
                        client.Send(payload);
                }
            }

            Console.WriteLine($"[NotificationServer] Broadcast message: {message}");
        }
    }
}
