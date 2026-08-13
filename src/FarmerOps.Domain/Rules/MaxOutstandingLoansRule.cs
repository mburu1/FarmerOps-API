using FarmerOps.Domain.Enums;

namespace FarmerOps.Domain.Rules;

/// <summary>A farmer may only carry one active (undischarged) loan at a time.</summary>
public sealed class MaxOutstandingLoansRule(int maxActiveLoans = 1) : ILoanEligibilityRule
{
    public string Code => "MaxOutstandingLoans";

    public LoanEligibilityRuleResult Evaluate(LoanEligibilityContext context)
    {
        var activeLoans = context.LoanHistory.Count(l =>
            l.Status is LoanStatus.Pending or LoanStatus.Approved or LoanStatus.Disbursed or LoanStatus.Overdue);

        return activeLoans < maxActiveLoans
            ? new LoanEligibilityRuleResult(true, "Farmer has no conflicting active loans.")
            : new LoanEligibilityRuleResult(false, $"Farmer already has {activeLoans} active loan(s); maximum allowed is {maxActiveLoans}.");
    }
}
