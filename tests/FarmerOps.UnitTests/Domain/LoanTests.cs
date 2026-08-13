using System.Reflection;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FarmerOps.UnitTests.Domain;

public class LoanTests
{
    private static Loan CreatePendingLoan(decimal principal = 10_000m) => new(Guid.NewGuid(), principal);

    /// <summary>
    /// Loan has no injectable clock, so backdating DueDateUtc for overdue-path tests requires
    /// reaching past the private setter via reflection rather than waiting on real time.
    /// </summary>
    private static void BackdateDueDate(Loan loan, DateTime dueDateUtc)
    {
        typeof(Loan).GetProperty(nameof(Loan.DueDateUtc))!.SetValue(loan, dueDateUtc);
    }

    [Fact]
    public void Constructor_WithNonPositivePrincipal_Throws()
    {
        var act = () => new Loan(Guid.NewGuid(), 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_FromPending_TransitionsToApprovedAndRaisesEvent()
    {
        var loan = CreatePendingLoan();

        loan.Approve();

        loan.Status.Should().Be(LoanStatus.Approved);
        loan.ApprovedAtUtc.Should().NotBeNull();
        loan.DomainEvents.Should().ContainSingle(e => e is LoanApprovedEvent);
    }

    [Fact]
    public void Approve_WhenNotPending_Throws()
    {
        var loan = CreatePendingLoan();
        loan.Approve();

        var act = loan.Approve;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_WithoutReason_Throws()
    {
        var loan = CreatePendingLoan();

        var act = () => loan.Reject(string.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_FromPending_SetsRejectedStatusAndReason()
    {
        var loan = CreatePendingLoan();

        loan.Reject("Insufficient collateral");

        loan.Status.Should().Be(LoanStatus.Rejected);
        loan.RejectionReason.Should().Be("Insufficient collateral");
        loan.DomainEvents.Should().ContainSingle(e => e is LoanRejectedEvent);
    }

    [Fact]
    public void Disburse_BeforeApproval_Throws()
    {
        var loan = CreatePendingLoan();

        var act = () => loan.Disburse(30);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Disburse_AfterApproval_SetsOutstandingBalanceAndDueDate()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();

        loan.Disburse(30);

        loan.Status.Should().Be(LoanStatus.Disbursed);
        loan.OutstandingBalance.Should().Be(10_000m);
        loan.DueDateUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
        loan.DomainEvents.Should().ContainSingle(e => e is LoanDisbursedEvent);
    }

    [Fact]
    public void RecordRepayment_ExceedingOutstandingBalance_Throws()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);

        var act = () => loan.RecordRepayment(10_000.01m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RecordRepayment_PartialAmount_ReducesBalanceWithoutMarkingRepaid()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);

        loan.RecordRepayment(4_000m);

        loan.OutstandingBalance.Should().Be(6_000m);
        loan.Status.Should().Be(LoanStatus.Disbursed);
    }

    [Fact]
    public void RecordRepayment_FullAmount_MarksLoanRepaid()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);

        loan.RecordRepayment(10_000m);

        loan.OutstandingBalance.Should().Be(0m);
        loan.Status.Should().Be(LoanStatus.Repaid);
        loan.RepaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void TryMarkOverdue_BeforeDueDate_ReturnsFalseAndLeavesStatusUnchanged()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);

        var result = loan.TryMarkOverdue();

        result.Should().BeFalse();
        loan.Status.Should().Be(LoanStatus.Disbursed);
    }

    [Fact]
    public void TryMarkOverdue_PastDueDate_FlagsOverdueAndRaisesEvent()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);
        BackdateDueDate(loan, DateTime.UtcNow.AddDays(-1));

        var result = loan.TryMarkOverdue();

        result.Should().BeTrue();
        loan.Status.Should().Be(LoanStatus.Overdue);
        loan.DomainEvents.Should().ContainSingle(e => e is LoanOverdueEvent);
    }

    [Fact]
    public void MarkDefaulted_WhenNotOverdue_Throws()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);

        var act = loan.MarkDefaulted;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkDefaulted_WhenOverdue_TransitionsToDefaulted()
    {
        var loan = CreatePendingLoan(10_000m);
        loan.Approve();
        loan.Disburse(30);
        BackdateDueDate(loan, DateTime.UtcNow.AddDays(-1));
        loan.TryMarkOverdue();

        loan.MarkDefaulted();

        loan.Status.Should().Be(LoanStatus.Defaulted);
    }
}
