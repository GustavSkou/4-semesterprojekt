using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
using System.Threading.Channels;

namespace PubSubControllerPlugin;

[ApiController]
[Route("ProductionSystem")]
public class EventsController : ControllerBase
{
    [HttpGet("Events")]
    public async Task GetEvents(CancellationToken ct)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        var channel = Channel.CreateUnbounded<string>();
        PubSubService.Subscribe(channel.Writer);

        try
        {
            await foreach (var message in channel.Reader.ReadAllAsync(ct))
            {
                await Response.WriteAsync($"data: {message}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            PubSubService.Unsubscribe(channel.Writer);
        }
    }
}