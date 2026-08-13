using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

public class Farmer : AggregateRoot
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string NationalId { get; private set; } = default!;
    public Guid DistrictId { get; private set; }
    public District? District { get; private set; }
    public decimal FarmSizeAcres { get; private set; }
    public CropType PrimaryCrop { get; private set; }
    public double? GeoLatitude { get; private set; }
    public double? GeoLongitude { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string FullName => $"{FirstName} {LastName}";

    private readonly List<Loan> _loans = [];
    public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();

    private Farmer()
    {
    }

    public Farmer(
        string firstName,
        string lastName,
        string phoneNumber,
        string nationalId,
        Guid districtId,
        decimal farmSizeAcres,
        CropType primaryCrop,
        double? geoLatitude = null,
        double? geoLongitude = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Farmer first name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Farmer last name is required.");
        if (farmSizeAcres <= 0)
            throw new DomainException("Farm size must be greater than zero.");

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        NationalId = nationalId;
        DistrictId = districtId;
        FarmSizeAcres = farmSizeAcres;
        PrimaryCrop = primaryCrop;
        GeoLatitude = geoLatitude;
        GeoLongitude = geoLongitude;

        Raise(new FarmerRegisteredEvent(Id, FullName, DistrictId));
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string phoneNumber,
        decimal farmSizeAcres,
        CropType primaryCrop,
        double? geoLatitude,
        double? geoLongitude)
    {
        if (farmSizeAcres <= 0)
            throw new DomainException("Farm size must be greater than zero.");

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        FarmSizeAcres = farmSizeAcres;
        PrimaryCrop = primaryCrop;
        GeoLatitude = geoLatitude;
        GeoLongitude = geoLongitude;
        Touch();
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
