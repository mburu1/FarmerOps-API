using FarmerOps.Domain.Entities;

namespace FarmerOps.Application.Regions.Dtos;

public sealed record DistrictDto(Guid Id, string Name, Guid RegionId)
{
    public static DistrictDto FromEntity(District district) => new(district.Id, district.Name, district.RegionId);
}

public sealed record RegionDto(Guid Id, string Name, string Code, IReadOnlyCollection<DistrictDto> Districts)
{
    public static RegionDto FromEntity(Region region) => new(
        region.Id,
        region.Name,
        region.Code,
        region.Districts.Select(DistrictDto.FromEntity).ToList());
}
