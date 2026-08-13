using FarmerOps.Domain.Entities;

namespace FarmerOps.Domain.Rules;

/// <summary>Snapshot of everything a loan eligibility rule needs to evaluate a farmer's application.</summary>
public sealed record LoanEligibilityContext(
    Farmer Farmer,
    decimal RequestedAmount,
    IReadOnlyCollection<Loan> LoanHistory);
