using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

/// <summary>
/// Loan state machine: Pending → Approved → Disbursed → Repaid, with Rejected, Overdue
/// and Defaulted as terminal/exceptional branches. Transitions are the only way to mutate
/// <see cref="Status"/>, so an invalid transition always throws rather than silently corrupting state.
/// </summary>
public class Loan : AggregateRoot
{
    public Guid FarmerId { get; private set; }
    public Farmer? Farmer { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public LoanStatus Status { get; private set; } = LoanStatus.Pending;
    public DateTime AppliedAtUtc { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public DateTime? DisbursedAtUtc { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public DateTime? RepaidAtUtc { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<InputOrder> _inputOrders = [];
    public IReadOnlyCollection<InputOrder> InputOrders => _inputOrders.AsReadOnly();

    private Loan()
    {
    }

    public Loan(Guid farmerId, decimal principalAmount)
    {
        if (principalAmount <= 0)
            throw new DomainException("Loan principal must be greater than zero.");

        FarmerId = farmerId;
        PrincipalAmount = principalAmount;
        AppliedAtUtc = DateTime.UtcNow;
    }

    public void Approve()
    {
        EnsureStatus(LoanStatus.Pending, nameof(Approve));

        Status = LoanStatus.Approved;
        ApprovedAtUtc = DateTime.UtcNow;
        Touch();

        Raise(new LoanApprovedEvent(Id, FarmerId, PrincipalAmount));
    }

    public void Reject(string reason)
    {
        EnsureStatus(LoanStatus.Pending, nameof(Reject));
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A rejection reason is required.");

        Status = LoanStatus.Rejected;
        RejectionReason = reason;
        Touch();

        Raise(new LoanRejectedEvent(Id, FarmerId, reason));
    }

    public void Disburse(int repaymentTermDays)
    {
        EnsureStatus(LoanStatus.Approved, nameof(Disburse));
        if (repaymentTermDays <= 0)
            throw new DomainException("Repayment term must be greater than zero days.");

        Status = LoanStatus.Disbursed;
        DisbursedAtUtc = DateTime.UtcNow;
        DueDateUtc = DisbursedAtUtc.Value.AddDays(repaymentTermDays);
        OutstandingBalance = PrincipalAmount;
        Touch();

        Raise(new LoanDisbursedEvent(Id, FarmerId, PrincipalAmount, DueDateUtc.Value));
    }

    public void RecordRepayment(decimal amount)
    {
        if (Status is not (LoanStatus.Disbursed or LoanStatus.Overdue))
            throw new DomainException($"Cannot record a repayment for a loan in status '{Status}'.");
        if (amount <= 0)
            throw new DomainException("Repayment amount must be greater than zero.");
        if (amount > OutstandingBalance)
            throw new DomainException("Repayment amount cannot exceed the outstanding balance.");

        OutstandingBalance -= amount;
        Touch();

        if (OutstandingBalance == 0)
        {
            Status = LoanStatus.Repaid;
            RepaidAtUtc = DateTime.UtcNow;
        }

        Raise(new LoanRepaymentRecordedEvent(Id, FarmerId, amount, OutstandingBalance));
    }

    /// <summary>Flags a disbursed loan as overdue once it passes its due date without full repayment.</summary>
    public bool TryMarkOverdue()
    {
        if (Status != LoanStatus.Disbursed || DueDateUtc is null || DateTime.UtcNow <= DueDateUtc)
            return false;

        Status = LoanStatus.Overdue;
        Touch();

        var daysOverdue = (DateTime.UtcNow - DueDateUtc.Value).Days;
        Raise(new LoanOverdueEvent(Id, FarmerId, OutstandingBalance, daysOverdue));
        return true;
    }

    public void MarkDefaulted()
    {
        EnsureStatus(LoanStatus.Overdue, nameof(MarkDefaulted));

        Status = LoanStatus.Defaulted;
        Touch();
    }

    private void EnsureStatus(LoanStatus expected, string operation)
    {
        if (Status != expected)
            throw new DomainException($"Cannot {operation} a loan in status '{Status}'; expected '{expected}'.");
    }
}
