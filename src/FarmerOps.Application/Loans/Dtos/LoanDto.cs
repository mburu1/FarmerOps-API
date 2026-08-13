using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Loans.Dtos;

public sealed record LoanDto(
    Guid Id,
    Guid FarmerId,
    string? FarmerName,
    decimal PrincipalAmount,
    decimal OutstandingBalance,
    LoanStatus Status,
    DateTime AppliedAtUtc,
    DateTime? ApprovedAtUtc,
    DateTime? DisbursedAtUtc,
    DateTime? DueDateUtc,
    DateTime? RepaidAtUtc,
    string? RejectionReason)
{
    public static LoanDto FromEntity(Loan loan) => new(
        loan.Id,
        loan.FarmerId,
        loan.Farmer?.FullName,
        loan.PrincipalAmount,
        loan.OutstandingBalance,
        loan.Status,
        loan.AppliedAtUtc,
        loan.ApprovedAtUtc,
        loan.DisbursedAtUtc,
        loan.DueDateUtc,
        loan.RepaidAtUtc,
        loan.RejectionReason);
}
