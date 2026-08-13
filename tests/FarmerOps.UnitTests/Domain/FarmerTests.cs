using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FarmerOps.UnitTests.Domain;

public class FarmerTests
{
    [Fact]
    public void Constructor_WithZeroFarmSize_Throws()
    {
        var act = () => new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 0m, CropType.Maize);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithBlankFirstName_Throws()
    {
        var act = () => new Farmer(" ", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 1m, CropType.Maize);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithValidData_RaisesFarmerRegisteredEvent()
    {
        var farmer = new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 2m, CropType.Maize);

        farmer.DomainEvents.Should().ContainSingle(e => e is FarmerRegisteredEvent);
        farmer.FullName.Should().Be("Jane Wanjiru");
        farmer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ThenReactivate_TogglesIsActive()
    {
        var farmer = new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 2m, CropType.Maize);

        farmer.Deactivate();
        farmer.IsActive.Should().BeFalse();

        farmer.Reactivate();
        farmer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_WithZeroFarmSize_Throws()
    {
        var farmer = new Farmer("Jane", "Wanjiru", "+254700000000", "12345678", Guid.NewGuid(), 2m, CropType.Maize);

        var act = () => farmer.UpdateProfile("Jane", "Wanjiru", "+254700000000", 0m, CropType.Maize, null, null);

        act.Should().Throw<DomainException>();
    }
}
