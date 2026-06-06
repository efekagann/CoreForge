namespace CoreForge.Application.Common.Interfaces;

public interface IBackgroundJobQueue
{
    ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem, CancellationToken ct = default);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct = default);
}
