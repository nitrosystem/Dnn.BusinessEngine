using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Core.PushingServer
{
    public class PushingServer
    {
        private readonly ConcurrentDictionary<string, List<Action<string>>> _subscribers =
            new ConcurrentDictionary<string, List<Action<string>>>();

        public void Subscribe(string channel, Action<string> handler)
        {
            _subscribers.AddOrUpdate(channel,
                _ => new List<Action<string>> { handler },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add(handler);
                    }
                    return list;
                });
        }

        public void Unsubscribe(string channel, Action<string> handler)
        {
            if (_subscribers.TryGetValue(channel, out var list))
            {
                lock (list)
                {
                    list.Remove(handler);
                }
            }
        }

        public void Publish(string channel, string message)
        {
            if (_subscribers.TryGetValue(channel, out var list))
            {
                List<Action<string>> handlers;
                lock (list)
                {
                    handlers = list.ToList();
                }

                foreach (var handler in handlers)
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception ex)
                    {
                        // خطاهای مربوط به Subscriber ها رو لاگ کن
                        Console.WriteLine($"[PushingServer] Error: {ex.Message}");
                    }
                }
            }
        }
    }

}
