using System.Threading.Channels;
using Energycom.Ingestion.Entities;

namespace Energycom.Ingestion.Ingest;

public class NotificationHub
{
    
    private HashSet<Channel<Reading>> _channels = new();

    public Task<Channel<Reading>> Subscribe()
    {
        
        var channel = Channel.CreateUnbounded<Reading>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        
        _channels.Add(channel);
        return Task.FromResult(channel);
    }
    
    
    public Task Unsubscribe(Channel<Reading> channel)
    {
        _channels.Remove(channel);
        return Task.CompletedTask;
    }
    
    public async Task Notify(Reading reading)
    {
        foreach (var channel in _channels)
        {
            await channel.Writer.WriteAsync(reading);
        }
    }
}