using FarmerOps.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmerOps.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job: drains unprocessed <c>OutboxMessages</c> and fans them out to webhook
/// subscribers. Runs frequently and in small batches so a subscriber outage never backs up
/// unbounded — failed messages stay unprocessed and are retried on the next run.
/// </summary>
public class OutboxProcessorJob(IApplicationDbContext db, IWebhookDispatcher webhookDispatcher, ILogger<OutboxProcessorJob> logger)
{
    private const int BatchSize = 50;

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            var delivered = await webhookDispatcher.DispatchAsync(message.Type, message.Content, cancellationToken);
            if (delivered)
                message.MarkProcessed();
            else
                message.MarkFailed("One or more webhook subscribers rejected or failed to receive the event.");
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Outbox processor dispatched {Count} message(s).", pending.Count);
    }
}
