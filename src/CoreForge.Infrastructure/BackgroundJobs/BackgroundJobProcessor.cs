using CoreForge.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreForge.Infrastructure.BackgroundJobs;

public class BackgroundJobProcessor(
    IBackgroundJobQueue queue,
    ILogger<BackgroundJobProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background job processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await queue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in background job.");
            }
        }

        logger.LogInformation("Background job processor stopped.");
    }
}
