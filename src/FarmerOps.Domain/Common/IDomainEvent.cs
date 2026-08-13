using MediatR;

namespace FarmerOps.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
