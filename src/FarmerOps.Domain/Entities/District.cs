using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Entities;

/// <summary>Mirrors a Kenyan sub-county within a Region (county).</summary>
public class District : BaseEntity
{
    public string Name { get; private set; } = default!;
    public Guid RegionId { get; private set; }
    public Region? Region { get; private set; }

    private District()
    {
    }

    public District(string name, Guid regionId)
    {
        Name = name;
        RegionId = regionId;
    }
}
