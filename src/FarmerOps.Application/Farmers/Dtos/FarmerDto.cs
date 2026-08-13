using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Farmers.Dtos;

public sealed record FarmerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string NationalId,
    Guid DistrictId,
    string? DistrictName,
    decimal FarmSizeAcres,
    CropType PrimaryCrop,
    double? GeoLatitude,
    double? GeoLongitude,
    bool IsActive,
    DateTime CreatedAtUtc)
{
    public static FarmerDto FromEntity(Farmer farmer) => new(
        farmer.Id,
        farmer.FirstName,
        farmer.LastName,
        farmer.PhoneNumber,
        farmer.NationalId,
        farmer.DistrictId,
        farmer.District?.Name,
        farmer.FarmSizeAcres,
        farmer.PrimaryCrop,
        farmer.GeoLatitude,
        farmer.GeoLongitude,
        farmer.IsActive,
        farmer.CreatedAtUtc);
}
