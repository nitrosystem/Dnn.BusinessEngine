using System.IO;

namespace NitroSystem.Dnn.BusinessEngine.Core.SseNotifier
{
    public class SseClient
    {
        public string Channel { get; set; }
        public StreamWriter Writer { get; set; }
    }
}
