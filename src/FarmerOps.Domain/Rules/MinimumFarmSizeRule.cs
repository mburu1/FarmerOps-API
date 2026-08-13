namespace FarmerOps.Domain.Rules;

/// <summary>Farms below this size cannot generate enough yield to reliably service a loan.</summary>
public sealed class MinimumFarmSizeRule(decimal minimumAcres = 0.25m) : ILoanEligibilityRule
{
    public string Code => "MinimumFarmSize";

    public LoanEligibilityRuleResult Evaluate(LoanEligibilityContext context)
    {
        return context.Farmer.FarmSizeAcres >= minimumAcres
            ? new LoanEligibilityRuleResult(true, "Farm size meets the minimum requirement.")
            : new LoanEligibilityRuleResult(false, $"Farm size ({context.Farmer.FarmSizeAcres} acres) is below the {minimumAcres}-acre minimum.");
    }
}
