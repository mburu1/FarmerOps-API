using FarmerOps.Domain.Enums;

namespace FarmerOps.Domain.Rules;

/// <summary>A prior default disqualifies a farmer from new credit until manually cleared.</summary>
public sealed class NoDefaultedLoanHistoryRule : ILoanEligibilityRule
{
    public string Code => "NoDefaultedLoanHistory";

    public LoanEligibilityRuleResult Evaluate(LoanEligibilityContext context)
    {
        var hasDefaulted = context.LoanHistory.Any(l => l.Status == LoanStatus.Defaulted);

        return !hasDefaulted
            ? new LoanEligibilityRuleResult(true, "No prior loan defaults on record.")
            : new LoanEligibilityRuleResult(false, "Farmer has a defaulted loan on record.");
    }
}
