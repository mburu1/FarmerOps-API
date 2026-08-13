namespace FarmerOps.Domain.Rules;

/// <summary>Caps loan size relative to farm size, keeping exposure proportional to repayment capacity.</summary>
public sealed class MaxRequestedAmountRule(decimal maxAmountPerAcre = 15_000m) : ILoanEligibilityRule
{
    public string Code => "MaxRequestedAmount";

    public LoanEligibilityRuleResult Evaluate(LoanEligibilityContext context)
    {
        var cap = context.Farmer.FarmSizeAcres * maxAmountPerAcre;

        return context.RequestedAmount <= cap
            ? new LoanEligibilityRuleResult(true, "Requested amount is within the per-acre cap.")
            : new LoanEligibilityRuleResult(false, $"Requested amount ({context.RequestedAmount:C}) exceeds the cap of {cap:C} for a {context.Farmer.FarmSizeAcres}-acre farm.");
    }
}
