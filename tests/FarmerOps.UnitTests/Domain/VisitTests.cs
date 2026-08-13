using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FarmerOps.UnitTests.Domain;

public class VisitTests
{
    private static Visit CreateScheduledVisit() => new(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

    [Fact]
    public void Complete_FromScheduled_SetsCompletedAndRaisesEvent()
    {
        var visit = CreateScheduledVisit();

        visit.Complete("All good");

        visit.Status.Should().Be(VisitStatus.Completed);
        visit.Notes.Should().Be("All good");
        visit.CompletedAtUtc.Should().NotBeNull();
        visit.DomainEvents.Should().ContainSingle(e => e is VisitCompletedEvent);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var visit = CreateScheduledVisit();
        visit.Complete(null);

        var act = () => visit.Complete(null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkMissed_FromScheduled_RaisesEvent()
    {
        var visit = CreateScheduledVisit();

        visit.MarkMissed();

        visit.Status.Should().Be(VisitStatus.Missed);
        visit.DomainEvents.Should().ContainSingle(e => e is VisitMissedEvent);
    }

    [Fact]
    public void Cancel_AfterCompletion_Throws()
    {
        var visit = CreateScheduledVisit();
        visit.Complete(null);

        var act = visit.Cancel;

        act.Should().Throw<DomainException>();
    }
}
