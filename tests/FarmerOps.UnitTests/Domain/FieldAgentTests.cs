using FarmerOps.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FarmerOps.UnitTests.Domain;

public class FieldAgentTests
{
    [Fact]
    public void RecordVisitOutcome_StartsAtMaxScore()
    {
        var agent = new FieldAgent("Peter", "Otieno", "+254711000000", "peter@farmerops.test");

        agent.PerformanceScore.Should().Be(100);
    }

    [Fact]
    public void RecordVisitOutcome_Completed_KeepsScoreAtMax()
    {
        var agent = new FieldAgent("Peter", "Otieno", "+254711000000", "peter@farmerops.test");

        agent.RecordVisitOutcome(completed: true);

        agent.PerformanceScore.Should().Be(100);
    }

    [Fact]
    public void RecordVisitOutcome_Missed_LowersScoreButDoesNotZeroItImmediately()
    {
        var agent = new FieldAgent("Peter", "Otieno", "+254711000000", "peter@farmerops.test");

        agent.RecordVisitOutcome(completed: false);

        agent.PerformanceScore.Should().BeLessThan(100).And.BeGreaterThan(0);
    }

    [Fact]
    public void RecordVisitOutcome_RepeatedMisses_TrendsTowardZero()
    {
        var agent = new FieldAgent("Peter", "Otieno", "+254711000000", "peter@farmerops.test");

        for (var i = 0; i < 50; i++)
            agent.RecordVisitOutcome(completed: false);

        agent.PerformanceScore.Should().BeLessThan(5);
    }
}
