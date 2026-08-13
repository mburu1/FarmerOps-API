using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

/// <summary>An outbound notification (SMS/webhook) queued for delivery through the mock gateway.</summary>
public class Alert : BaseEntity
{
    public Guid? FarmerId { get; private set; }
    public Guid? AgentId { get; private set; }
    public AlertType Type { get; private set; }
    public string Message { get; private set; } = default!;
    public AlertStatus Status { get; private set; } = AlertStatus.Pending;
    public DateTime? SentAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public int AttemptCount { get; private set; }

    private Alert()
    {
    }

    public Alert(AlertType type, string message, Guid? farmerId = null, Guid? agentId = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Alert message is required.");
        if (farmerId is null && agentId is null)
            throw new DomainException("An alert must target a farmer, an agent, or both.");

        Type = type;
        Message = message;
        FarmerId = farmerId;
        AgentId = agentId;
    }

    public void MarkSent()
    {
        Status = AlertStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        AttemptCount++;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        Status = AlertStatus.Failed;
        FailureReason = reason;
        AttemptCount++;
        Touch();
    }
}
