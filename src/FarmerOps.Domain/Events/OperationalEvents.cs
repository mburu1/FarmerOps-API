using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Events;

public sealed record VisitCompletedEvent(Guid VisitId, Guid AgentId, Guid FarmerId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record VisitMissedEvent(Guid VisitId, Guid AgentId, Guid FarmerId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record InputOrderFulfilledEvent(Guid InputOrderId, Guid FarmerId, decimal TotalCost) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record FarmerRegisteredEvent(Guid FarmerId, string FullName, Guid DistrictId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
