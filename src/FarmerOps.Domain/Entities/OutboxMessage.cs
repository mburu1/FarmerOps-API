using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Entities;

/// <summary>
/// Transactional outbox entry: domain events are persisted in the same DB transaction as the
/// business change, then dispatched (webhooks, integration events) by a background processor.
/// This guarantees at-least-once delivery without a distributed transaction.
/// </summary>
public class OutboxMessage : BaseEntity
{
    public string Type { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    private OutboxMessage()
    {
    }

    public OutboxMessage(string type, string content, DateTime occurredOnUtc)
    {
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
