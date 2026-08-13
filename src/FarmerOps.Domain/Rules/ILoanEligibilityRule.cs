namespace FarmerOps.Domain.Rules;

/// <summary>
/// A single eligibility check. Each implementation is independent and self-contained so the
/// rule set can grow (or be reconfigured per program) without touching the engine or other rules.
/// </summary>
public interface ILoanEligibilityRule
{
    /// <summary>Short, stable identifier surfaced in the eligibility report (e.g. "MinimumFarmSize").</summary>
    string Code { get; }

    LoanEligibilityRuleResult Evaluate(LoanEligibilityContext context);
}

public sealed record LoanEligibilityRuleResult(bool Passed, string Reason);
