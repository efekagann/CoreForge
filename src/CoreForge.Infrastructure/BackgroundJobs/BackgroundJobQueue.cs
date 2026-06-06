using CoreForge.Application.Common.Interfaces;
using System.Threading.Channels;

namespace CoreForge.Infrastructure.BackgroundJobs;

public class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _channel =
        Channel.CreateBounded<Func<CancellationToken, ValueTask>>(
            new BoundedChannelOptions(capacity: 100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

    public async ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _channel.Writer.WriteAsync(workItem, ct);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct = default)
        => await _channel.Reader.ReadAsync(ct);
}
