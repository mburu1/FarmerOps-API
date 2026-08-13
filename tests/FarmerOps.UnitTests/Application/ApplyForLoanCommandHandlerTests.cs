using FarmerOps.Application.Loans.Commands.ApplyForLoan;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Rules;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace FarmerOps.UnitTests.Application;

public class ApplyForLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_EligibleFarmer_CreatesPendingLoan()
    {
        await using var db = TestDbContextFactory.Create();
        var farmer = new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 2m, CropType.Maize);
        db.Farmers.Add(farmer);
        await db.SaveChangesAsync();

        var handler = new ApplyForLoanCommandHandler(db, LoanEligibilityEngine.CreateDefault());

        var result = await handler.Handle(new ApplyForLoanCommand(farmer.Id, 10_000m), CancellationToken.None);

        result.Status.Should().Be(LoanStatus.Pending);
        db.Loans.Should().ContainSingle(l => l.FarmerId == farmer.Id);
    }

    [Fact]
    public async Task Handle_IneligibleFarmer_ThrowsValidationExceptionAndDoesNotPersistLoan()
    {
        await using var db = TestDbContextFactory.Create();
        var farmer = new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 0.1m, CropType.Maize); // below minimum
        db.Farmers.Add(farmer);
        await db.SaveChangesAsync();

        var handler = new ApplyForLoanCommandHandler(db, LoanEligibilityEngine.CreateDefault());

        var act = () => handler.Handle(new ApplyForLoanCommand(farmer.Id, 1_000m), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        db.Loans.Should().BeEmpty();
    }
}
