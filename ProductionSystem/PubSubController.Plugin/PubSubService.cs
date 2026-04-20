using System.Text.Json;
using System.Threading.Channels;
using Common.Data;
using Common.PubSubDataSource;

namespace PubSubControllerPlugin;

public class PubSubService : IPubSubDataSource
{
    private static readonly List<ChannelWriter<string>> _subscribers = new();
    private static readonly object _lock = new();

    public void Publish(ProductionEvent e)
    {
        string json = JsonSerializer.Serialize(e);
        lock (_lock)
        {
            foreach (var writer in _subscribers.ToList())
                writer.TryWrite(json);
        }
    }

    public static void Subscribe(ChannelWriter<string> writer)
    {
        lock (_lock) _subscribers.Add(writer);
    }

    public static void Unsubscribe(ChannelWriter<string> writer)
    {
        lock (_lock) _subscribers.Remove(writer);
    }
    
}
