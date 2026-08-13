namespace FarmerOps.Domain.Rules;

/// <summary>
/// Runs every registered rule against a candidate loan application. Rules run in full (no
/// short-circuiting) so the caller gets a complete eligibility report rather than the first failure.
/// </summary>
public sealed class LoanEligibilityEngine(IEnumerable<ILoanEligibilityRule> rules)
{
    public static LoanEligibilityEngine CreateDefault() => new(
    [
        new MinimumFarmSizeRule(),
        new MaxOutstandingLoansRule(),
        new NoDefaultedLoanHistoryRule(),
        new MaxRequestedAmountRule()
    ]);

    public LoanEligibilityReport Evaluate(LoanEligibilityContext context)
    {
        var results = rules
            .Select(rule => new LoanEligibilityRuleOutcome(rule.Code, rule.Evaluate(context)))
            .ToList();

        var isEligible = results.All(r => r.Result.Passed);
        return new LoanEligibilityReport(isEligible, results);
    }
}

public sealed record LoanEligibilityRuleOutcome(string RuleCode, LoanEligibilityRuleResult Result);

public sealed record LoanEligibilityReport(bool IsEligible, IReadOnlyCollection<LoanEligibilityRuleOutcome> RuleOutcomes);
