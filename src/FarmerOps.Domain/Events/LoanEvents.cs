using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Events;

public sealed record LoanApprovedEvent(Guid LoanId, Guid FarmerId, decimal PrincipalAmount) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record LoanDisbursedEvent(Guid LoanId, Guid FarmerId, decimal PrincipalAmount, DateTime DueDateUtc) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record LoanRepaymentRecordedEvent(Guid LoanId, Guid FarmerId, decimal AmountPaid, decimal OutstandingBalance) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record LoanOverdueEvent(Guid LoanId, Guid FarmerId, decimal OutstandingBalance, int DaysOverdue) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record LoanRejectedEvent(Guid LoanId, Guid FarmerId, string Reason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
