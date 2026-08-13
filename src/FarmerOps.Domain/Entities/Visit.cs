using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

public class Visit : AggregateRoot
{
    public Guid AgentId { get; private set; }
    public FieldAgent? Agent { get; private set; }
    public Guid FarmerId { get; private set; }
    public Farmer? Farmer { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public VisitStatus Status { get; private set; } = VisitStatus.Scheduled;
    public string? Notes { get; private set; }

    private Visit()
    {
    }

    public Visit(Guid agentId, Guid farmerId, DateTime scheduledAtUtc)
    {
        AgentId = agentId;
        FarmerId = farmerId;
        ScheduledAtUtc = scheduledAtUtc;
    }

    public void Complete(string? notes)
    {
        if (Status != VisitStatus.Scheduled)
            throw new DomainException($"Cannot complete a visit in status '{Status}'.");

        Status = VisitStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        Notes = notes;
        Touch();

        Raise(new VisitCompletedEvent(Id, AgentId, FarmerId));
    }

    public void MarkMissed()
    {
        if (Status != VisitStatus.Scheduled)
            throw new DomainException($"Cannot mark a visit in status '{Status}' as missed.");

        Status = VisitStatus.Missed;
        Touch();

        Raise(new VisitMissedEvent(Id, AgentId, FarmerId));
    }

    public void Cancel()
    {
        if (Status != VisitStatus.Scheduled)
            throw new DomainException($"Cannot cancel a visit in status '{Status}'.");

        Status = VisitStatus.Cancelled;
        Touch();
    }
}
