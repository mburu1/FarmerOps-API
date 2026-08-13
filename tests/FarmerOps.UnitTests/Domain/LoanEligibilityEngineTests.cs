using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Rules;
using FluentAssertions;
using Xunit;

namespace FarmerOps.UnitTests.Domain;

public class LoanEligibilityEngineTests
{
    private static Farmer CreateFarmer(decimal farmSizeAcres = 2m) => new(
        "Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), farmSizeAcres, CropType.Maize);

    [Fact]
    public void Evaluate_WithNoPriorLoansAndReasonableRequest_IsEligible()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer(farmSizeAcres: 2m);
        var context = new LoanEligibilityContext(farmer, RequestedAmount: 10_000m, LoanHistory: []);

        var report = engine.Evaluate(context);

        report.IsEligible.Should().BeTrue();
        report.RuleOutcomes.Should().OnlyContain(o => o.Result.Passed);
    }

    [Fact]
    public void Evaluate_WithFarmBelowMinimumSize_FailsMinimumFarmSizeRule()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer(farmSizeAcres: 0.1m);
        var context = new LoanEligibilityContext(farmer, RequestedAmount: 1_000m, LoanHistory: []);

        var report = engine.Evaluate(context);

        report.IsEligible.Should().BeFalse();
        report.RuleOutcomes.Should().Contain(o => o.RuleCode == "MinimumFarmSize" && !o.Result.Passed);
    }

    [Fact]
    public void Evaluate_WithExistingActiveLoan_FailsMaxOutstandingLoansRule()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer();
        var activeLoan = new Loan(farmer.Id, 5_000m); // Pending counts as active
        var context = new LoanEligibilityContext(farmer, RequestedAmount: 5_000m, LoanHistory: [activeLoan]);

        var report = engine.Evaluate(context);

        report.IsEligible.Should().BeFalse();
        report.RuleOutcomes.Should().Contain(o => o.RuleCode == "MaxOutstandingLoans" && !o.Result.Passed);
    }

    [Fact]
    public void Evaluate_WithPriorDefault_FailsNoDefaultedLoanHistoryRule()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer();
        var defaultedLoan = new Loan(farmer.Id, 5_000m);
        defaultedLoan.Approve();
        defaultedLoan.Disburse(30);
        typeof(Loan).GetProperty(nameof(Loan.DueDateUtc))!.SetValue(defaultedLoan, DateTime.UtcNow.AddDays(-1));
        defaultedLoan.TryMarkOverdue();
        defaultedLoan.MarkDefaulted();

        var context = new LoanEligibilityContext(farmer, RequestedAmount: 5_000m, LoanHistory: [defaultedLoan]);

        var report = engine.Evaluate(context);

        report.IsEligible.Should().BeFalse();
        report.RuleOutcomes.Should().Contain(o => o.RuleCode == "NoDefaultedLoanHistory" && !o.Result.Passed);
    }

    [Fact]
    public void Evaluate_WithAmountExceedingPerAcreCap_FailsMaxRequestedAmountRule()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer(farmSizeAcres: 1m); // cap = 15,000 at the default 15,000/acre rate
        var context = new LoanEligibilityContext(farmer, RequestedAmount: 20_000m, LoanHistory: []);

        var report = engine.Evaluate(context);

        report.IsEligible.Should().BeFalse();
        report.RuleOutcomes.Should().Contain(o => o.RuleCode == "MaxRequestedAmount" && !o.Result.Passed);
    }

    [Fact]
    public void Evaluate_RunsAllRulesEvenAfterAFailure_NoShortCircuiting()
    {
        var engine = LoanEligibilityEngine.CreateDefault();
        var farmer = CreateFarmer(farmSizeAcres: 0.1m); // fails MinimumFarmSize
        var context = new LoanEligibilityContext(farmer, RequestedAmount: 999_999m, LoanHistory: []); // also fails MaxRequestedAmount

        var report = engine.Evaluate(context);

        report.RuleOutcomes.Should().HaveCount(4);
        report.RuleOutcomes.Count(o => !o.Result.Passed).Should().Be(2);
    }
}
