using FarmerOps.Domain.Rules;

namespace FarmerOps.Application.Loans.Dtos;

public sealed record LoanEligibilityRuleOutcomeDto(string RuleCode, bool Passed, string Reason);

public sealed record LoanEligibilityReportDto(bool IsEligible, IReadOnlyCollection<LoanEligibilityRuleOutcomeDto> RuleOutcomes)
{
    public static LoanEligibilityReportDto FromDomain(LoanEligibilityReport report) => new(
        report.IsEligible,
        report.RuleOutcomes.Select(o => new LoanEligibilityRuleOutcomeDto(o.RuleCode, o.Result.Passed, o.Result.Reason)).ToList());
}
