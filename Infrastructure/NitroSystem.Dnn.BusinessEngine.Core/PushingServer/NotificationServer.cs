using System;
using System.Linq;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class NotificationServer : WebSocketBehavior
    {
        public static readonly ConcurrentDictionary<string, ConcurrentBag<WebSocket>> Channels = new();
        public static readonly ConcurrentDictionary<WebSocket, DateTime> LastSeen = new();

        private string _channelName;

        protected override void OnOpen()
        {
            _channelName = Context.QueryString["channel"] ?? "default";
            var socket = Context.WebSocket;

            Channels.AddOrUpdate(
                _channelName,
                _ => new ConcurrentBag<WebSocket>() { socket },
                (_, bag) =>
                {
                    bag.Add(socket);
                    return bag;
                }
            );

            LastSeen[socket] = DateTime.UtcNow;

            Console.WriteLine($"[WebSocket] Client connected to channel '{_channelName}'");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            LastSeen[Context.WebSocket] = DateTime.UtcNow;

            Console.WriteLine($"[WebSocket] Message from '{_channelName}': {e.Data}");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            CleanupSocket(Context.WebSocket);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            CleanupSocket(Context.WebSocket);
        }

        private void CleanupSocket(WebSocket socket)
        {
            LastSeen.TryRemove(socket, out _);

            foreach (var bag in Channels.Values)
            {
                var list = bag.ToList();
                list.Remove(socket);

                // پاکسازی کامل
                while (!bag.IsEmpty)
                    bag.TryTake(out _);

                foreach (var s in list)
                    bag.Add(s);
            }

            Console.WriteLine("[WebSocket] Client removed.");
        }

        public static void SendToChannel(string channel, object data)
        {
            if (!Channels.TryGetValue(channel, out var clients))
                return;

            var payload = JsonConvert.SerializeObject(new
            {
                Channel = channel,
                Message = data
            });

            foreach (var client in clients)
            {
                if (client.ReadyState == WebSocketState.Open)
                    client.Send(payload);
            }
        }
    }

}
