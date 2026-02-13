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
using Newtonsoft.Json;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    public class SseController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Notify(string channel)
        {
            var cancellation = HttpContext.Current.Response.ClientDisconnectedToken;

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");
            response.Headers.Add("X-Accel-Buffering", "no");

            response.Content = new PushStreamContent(
                async (stream, content, context) =>
                {
                    HttpContext.Current.Response.BufferOutput = false;

                    using (var writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;

                        var client = new SseClient
                        {
                            Channel = channel,
                            Writer = writer
                        };

                        SseConnectionManager.AddClient(client);

                        try
                        {
                            // handshake
                            await writer.WriteAsync(
                                "event: ready\ndata: {\"status\":\"ok\",\"ts\":\""
                                + DateTime.UtcNow.ToString("O") + "\"}\n\n"
                            );

                            // heartbeat loop
                            while (!cancellation.IsCancellationRequested)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(5), cancellation);

                                // SSE comment heartbeat
                                await writer.WriteAsync(": ping\n\n");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // client disconnected – expected
                        }
                        catch (Exception)
                        {
                            // log if needed
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
