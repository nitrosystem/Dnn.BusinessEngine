using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    public class SseController : DnnApiController
    {
        [HttpGet]
        [Route("api/sse/notify")]
        public HttpResponseMessage Notify(string channel)
        {
            var cancellation = HttpContext.Current.Response.ClientDisconnectedToken;
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Cache-Control", "no-cache");

            response.Content = new PushStreamContent(
                async (stream, content, transportContext) =>
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        var client = new SseClient
                        {
                            Channel = channel,
                            Writer = writer
                        };

                        SseConnectionManager.AddClient(client);

                        try
                        {
                            // منتظر قطع اتصال کلاینت
                            await Task.Delay(Timeout.Infinite, cancellation);
                        }
                        catch (OperationCanceledException)
                        {
                            // connection بسته شد
                        }
                        finally
                        {
                            SseConnectionManager.RemoveClient(client.Channel);
                        }
                    }
                },
                "text/event-stream"
            );

            return response;
        }
    }
}
